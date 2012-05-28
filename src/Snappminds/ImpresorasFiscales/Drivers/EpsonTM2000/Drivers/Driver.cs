using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Timers;
using System.Threading;
using System.Text;

namespace Snappminds.ImpresorasFiscales.Drivers.EpsonTM2000
{
	enum CommunicationCodes
	{
		STX = 0x02,
		ETX = 0x03,
		DC2 = 0x12,
		DC4 = 0x14,
		NAK = 0x15
	};
	
	public enum ConnectionStates
	{
		OPEN,
		CLOSED,
		SENDING_DATA,
		WAITING_DATA,
		ERROR
	}
	
	public enum ConnectionErrors
	{
		SEND_BUFFER_FULL_ERROR,
		RECEIVE_BUFFER_FULL_ERROR,
		BUFFER_OVERRUN_ERROR,
		RECEIVE_PARITY_ERROR,
		FRAMING_ERROR,
		TIMEOUT_ERROR		
	}
	
	public class ErrorReceivedEventArgs : EventArgs
	{
		private ConnectionErrors _Error;
		
		public ErrorReceivedEventArgs(ConnectionErrors error)
		{
			this.Error = error;
		}
		
		public ConnectionErrors Error {
			protected set {
				this._Error = value;
			}
			
			get {
				return this._Error;
			}
		}		
	}
	
	public class MessageReceviedEventArgs : EventArgs
	{
		private byte[] _Message;
		
		public MessageReceviedEventArgs(byte[] message)
		{
			this.Message = message;
		}
		
		public byte[] Message {
			protected set {
				this._Message = value;
			}
			
			get {
				return this._Message;
			}
		}
	}
	
	/// <summary>
	/// Conexion con una impresora fiscal.
	/// TODO: Reintentar envios al recibir NAK.
	/// TODO: Devolver NAK al recibir checksum invalido.
	/// </summary>
	public class Driver
	{
		protected ConnectionStates State;
		private Nullable<byte> _ReadBuffer;
		private SerialPort _Port;
		private byte _SequenceNumber;
		private System.Timers.Timer _TimeOutTimer;
		
		public event EventHandler<MessageReceviedEventArgs> MessageReceived;
		public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
		
		public Driver(String portName)
		{
			this.State = ConnectionStates.CLOSED;
			this.Port = new SerialPort(portName);
			this.Port.WriteTimeout = 800;
			this.Port.ReadTimeout = 800;
			this.Port.BaudRate = 9600;			
			this.ReadBuffer = null;

			this.TimeOutTimer = new System.Timers.Timer(800);
			this.TimeOutTimer.Elapsed += this.TimeOutTimer_Elapsed;
		}
		
		public int BaudRate {
			set {
				this.Port.BaudRate = value;
			}
			get {
				return this.Port.BaudRate;
			}
		}
		
		protected Nullable<byte> ReadBuffer {
			set {
				this._ReadBuffer = value;
			}
			get {
				return this._ReadBuffer;
			}
		}
		
		protected SerialPort Port {
			set {
				this._Port = value;
			}
			get {
				return this._Port;
			}
		}
		
		protected Byte SequenceNumber {
			set {
				this._SequenceNumber = value;
			}
			get {
				return this._SequenceNumber;
			}
		}
		
		protected System.Timers.Timer TimeOutTimer {
			set {
				this._TimeOutTimer = value;
			}
			get {
				return this._TimeOutTimer;
			}
		}
		
		public void Open()
		{
			lock (this) {
				if (this.State == ConnectionStates.CLOSED) {
					this.Port.Open();
					this.State = ConnectionStates.OPEN;
					this.SequenceNumber = 0x20;
					Thread t = new Thread(new ThreadStart(SerialPortReadPolling));
					t.Start();
				}
			}
		}
		
		public void Close()
		{
			lock (this) {
				this.Port.Close();
				this.State = ConnectionStates.CLOSED;
			}
		}
		
		/// <summary>
		/// Envia un frame de datos a la impresora fiscal.
		/// Los datos a enviar no deben poseer registros de encabezado, solo de datos.
		/// </summary>
		/// <param name='datos'>
		/// Datos a enviar.
		/// </param>
		public void Send(byte[] data)
		{

			List<byte> frame = new List<byte>();					
			
			//STX
			frame.Add((byte)CommunicationCodes.STX);	
			//Seq
			frame.Add(this.SequenceNumber);			
			
			frame.AddRange(data);
			
			//ETX
			frame.Add((byte)CommunicationCodes.ETX);
			
			//Checksum (BCC)
			String hexCheckSum = this.GenerateCheckSum(frame.ToArray()).ToString("X");
			hexCheckSum = hexCheckSum.PadLeft(4, '0');
			
			frame.Add((byte)hexCheckSum[0]);
			frame.Add((byte)hexCheckSum[1]);
			frame.Add((byte)hexCheckSum[2]);
			frame.Add((byte)hexCheckSum[3]);
			
			
			//Intentar envio
			this.PortWrite(frame.ToArray(), 0, frame.Count);		

		}
		
		protected void PortWrite(byte[] data, int index, int count)
		{
			lock (this) {				
				if (this.State == ConnectionStates.OPEN) {
					
					this.State = ConnectionStates.SENDING_DATA;
							
					try {
						this.Port.Write(data, 0, count);
					} catch (Exception) {
						this.State = ConnectionStates.ERROR;
					}
					
					//Si se logro enviar y no hubo timeout, esperar la respuesta.
					if (this.State == ConnectionStates.SENDING_DATA) {					
						this.State = ConnectionStates.WAITING_DATA;
						this.TimeOutTimer.Start();	
					}
				}
			}
		}
		
		protected byte PortReadByte()
		{

			byte result;
			if (!this.ReadBuffer.HasValue) {
				result = (byte)this.Port.ReadByte();
			} else {				
				result = this.ReadBuffer.Value;
				this.ReadBuffer = null;
			}
			

			return result;
		}
		
		protected byte PortPeekByte()
		{			
			if (this.ReadBuffer == null) {
				this.ReadBuffer = (byte)this.Port.ReadByte();
			}
			
			return this.ReadBuffer.Value;
		}
		

		
		/// <summary>
		/// Calcula el checksum del frame de datos.
		/// </summary>
		/// <returns>
		/// Checksum del frame de datos.
		/// </returns>
		/// <param name='data'>
		/// Frame de datos.
		/// </param>
		protected Int16 GenerateCheckSum(byte[] data)
		{
			Int16 sum = 0;
			foreach (byte b in data) {
				sum += b;
			}
			
			return sum;
		}

		
		/// <summary>
		/// Se produce cuando el puerto recibe informacion.
		/// </summary>
		protected void OnSerialPortDataReceived()
		{
			lock (this) {
				this.TimeOutTimer.Stop();
				
				while (this.Port.BytesToRead > 0) {
					byte code = (byte)this.PortPeekByte();
					switch (code) {
					case (byte)CommunicationCodes.STX:
						if (this.State == ConnectionStates.WAITING_DATA) {
							this.TimeOutTimer.Stop();
							byte[] messageData = this.ProcessFrame();																						
							this.OnMessageReceived(new MessageReceviedEventArgs(messageData));

						} else {
							//Si no se estaba esperando un mensaje entonces descartar los datos.
							this.PortReadByte();
						}
						break;
					case (byte)CommunicationCodes.ETX:
						this.PortReadByte();
						this.TimeOutTimer.Stop();
						
						break;
					case (byte)CommunicationCodes.DC2:
						this.PortReadByte();
						this.TimeOutTimer.Stop();
						this.TimeOutTimer.Start();	
						
						break;
					case (byte)CommunicationCodes.DC4:
						this.PortReadByte();
						this.TimeOutTimer.Stop();
						this.TimeOutTimer.Start();	
						
						break;
					case (byte)CommunicationCodes.NAK:
						this.PortReadByte();
						this.TimeOutTimer.Stop();
						
						break;
					default:
						this.PortReadByte();
					
						break;
					}
					
				}
			}
			
		}
		
		
		/// <summary>
		/// Lee, procesa, y devuelve el valor del checksum a partir del puerto.
		/// </summary>
		/// <returns>
		/// El checksum leido.
		/// </returns>
		protected Int16 ProcessCheckSum()
		{
			List<byte> checksum = new List<byte>();
			
			//CheckSum
			for (int i=1; i<=4; i++) {
				checksum.Add(this.PortReadByte());
			}
			
			
			return Int16.Parse(
				Encoding.ASCII.GetString(checksum.ToArray()),
				System.Globalization.NumberStyles.AllowHexSpecifier
			);
			

		}
		
		/// <summary>
		/// Procesa, valida y devuelve los datos contenidos en un frame.
		/// </summary>
		/// <returns>
		/// El frame de datos.
		/// </returns>
		protected byte[] ProcessFrame()
		{
			
			byte code;
			List<byte> frame = new List<byte>(); //Frame sin checksum
			List<byte> message = new List<byte>(); //Frame de datos
			
			code = this.PortReadByte();
			if (code != (byte)CommunicationCodes.STX) 
				throw new Exception("El formato del frame es incorrecto. Se esperaba STX.");			
			
			frame.Add(code);
			
			code = this.PortReadByte();
			if (code != this.SequenceNumber) 
				throw new Exception(
					String.Format(
					"El numero de secuencia es incorrecto. Se esperaba {0} pero se obtuvo {1}",
					this.SequenceNumber.ToString("X"), 
					code.ToString("X")
				)
			);
			
			frame.Add(code);
			
			while ((code = this.PortReadByte()) != (byte)CommunicationCodes.ETX) {
				message.Add(code);
				frame.Add(code);
			}
			
			frame.Add(code);			
			
			//Comprobar checksum
			if (this.ProcessCheckSum() != this.GenerateCheckSum(frame.ToArray()))
				throw new Exception("El checksum del frame es inválido.");
			
			
			return message.ToArray();

		}
		
		/// <summary>
		/// EventHandler para el evento Elapsed del timer de timeout de envio/recepcion.
		/// </summary>
		/// <param name='sender'>
		/// Timer que genero el evento.
		/// </param>
		/// <param name='e'>
		/// Argumentos del evento.
		/// </param>
		protected void TimeOutTimer_Elapsed(Object sender, ElapsedEventArgs e)
		{						
			/*
			 * Este evento se levanta en otro thread. Es posible que al mismo tiempo se haya terminado de
			 * realizar el envio/recepcion. Por este motive se vuelve a chequear el estado.
			*/
			lock (this) {
				Console.WriteLine("TIMEOUT! - STATE: {0}", this.State.ToString());
				this.TimeOutTimer.Stop(); //Detener el timer para evitar que se siga produciendo el evento.
				
				if ((this.State == ConnectionStates.SENDING_DATA) || (this.State == ConnectionStates.WAITING_DATA)) {
					
					this.State = ConnectionStates.ERROR;
					
					//En caso de timeout por envio de informacion, limpiar el buffer para finalizar la llamada al write.
					if (this.State == ConnectionStates.SENDING_DATA)
						this.Port.DiscardOutBuffer();
					
					this.OnErrorReceived(new ErrorReceivedEventArgs(ConnectionErrors.TIMEOUT_ERROR));
					
					this.State = ConnectionStates.OPEN;
				}
			}			
		}
		
		/// <summary>
		/// EventHandler para el evento de errores del puerto serie.
		/// TODO: Chequear que no existan problemas de concurrencia.
		/// </summary>
		/// <param name='sender'>
		/// Puerto que genero el evento.
		/// </param>
		/// <param name='e'>
		/// Argumentos del evento.
		/// </param>
		protected void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		{
			
			ConnectionErrors error = ConnectionErrors.FRAMING_ERROR;
			
			switch (e.EventType) {
			case SerialError.Frame:
				error = ConnectionErrors.FRAMING_ERROR;
				break;
			case SerialError.Overrun:
				error = ConnectionErrors.BUFFER_OVERRUN_ERROR;
				break;
			case SerialError.RXOver:
				error = ConnectionErrors.RECEIVE_BUFFER_FULL_ERROR;
				break;
			case SerialError.RXParity:
				error = ConnectionErrors.RECEIVE_PARITY_ERROR;
				break;
			case SerialError.TXFull:
				error = ConnectionErrors.SEND_BUFFER_FULL_ERROR;
				break;
			}
			
			Console.WriteLine("PORT-ERROR!  MESSAGE: {0}", error.ToString());
			
			this.OnErrorReceived(new ErrorReceivedEventArgs(error));
		}
		
		protected void OnErrorReceived(ErrorReceivedEventArgs e)
		{
			Console.WriteLine("ERROR!  MESSAGE: {0}", e.Error.ToString());
			
			if (this.ErrorReceived != null) {
				this.ErrorReceived(this, e);
			}
		}
		
		protected void OnMessageReceived(MessageReceviedEventArgs e)
		{
			lock (this) {
				this.State = ConnectionStates.OPEN;
				this.SequenceNumber++;
				
				if (this.MessageReceived != null) {
					this.MessageReceived(this, e);
				}
			}
		}
		
		/// <summary>
		/// Se realiza polling para comprobar que haya informacion en el buffer de entrada.
		/// En MONO no funciona el uso de eventos para recepcion de datos.
		/// Este metodo se ejecuta en un thread aparte.
		/// </summary>
		protected void SerialPortReadPolling()
		{
			int actualBytesToRead = 0;
			while (this.Port.IsOpen) {
				try {
					if (this.Port.BytesToRead > actualBytesToRead) {												
						this.OnSerialPortDataReceived();
						actualBytesToRead = this.Port.BytesToRead;
					}
				} catch (InvalidOperationException) {
					//Ignorar excepcion.
				} catch (IOException) {
					this.OnErrorReceived(new ErrorReceivedEventArgs(ConnectionErrors.TIMEOUT_ERROR));
				} catch (Exception) {
				}
			}
		}
	}
}

