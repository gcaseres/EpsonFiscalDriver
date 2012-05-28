using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Timers;
using System.Threading;
using System.Text;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Messages;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Drivers
{
	enum CommunicationCodes
	{
		STX = 0x02,
		ETX = 0x03,
		DC2 = 0x12,
		DC4 = 0x14,
		NAK = 0x15
	}
	;

	public enum ConnectionErrors
	{
		SEND_BUFFER_FULL_ERROR,
		RECEIVE_BUFFER_FULL_ERROR,
		BUFFER_OVERRUN_ERROR,
		RECEIVE_PARITY_ERROR,
		FRAMING_ERROR,
		TIMEOUT_ERROR
	}

	class NegativeAcknowledgeException : Exception
	{
		public NegativeAcknowledgeException() : base()
		{
		}
	}

	class InvalidChecksumException : Exception
	{
		public InvalidChecksumException() : base()
		{
		}
	}

	class InvalidSequenceNumberException : Exception
	{
		private byte _ExpectedSequenceNumber;
		private byte _CurrentSequenceNumber;

		public InvalidSequenceNumberException(byte currentSequenceNumber, byte expectedSequenceNumber) : base()
		{
			this.CurrentSequenceNumber = currentSequenceNumber;
			this.ExpectedSequenceNumber = expectedSequenceNumber;
		}

		public byte CurrentSequenceNumber {
			protected set {
				this._CurrentSequenceNumber = value;
			}
			get {
				return this._CurrentSequenceNumber;
			}
		}

		public byte ExpectedSequenceNumber {
			protected set {
				this._ExpectedSequenceNumber = value;
			}
			get {
				return this._ExpectedSequenceNumber;
			}
		}

		public override string Message {
			get {
				return String.Format(
					"El numero de secuencia es incorrecto. Se esperaba {0} pero se obtuvo {1}",
					this.ExpectedSequenceNumber.ToString("X"), 
					this.CurrentSequenceNumber.ToString("X")
				);
			}
		}
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
		private byte[] _MessageData;
		
		public MessageReceviedEventArgs(byte[] message)
		{
			this.MessageData = message;
		}
		
		public byte[] MessageData {
			protected set {
				this._MessageData = value;
			}
			
			get {
				return this._MessageData;
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
		private ConnectionErrors _LastError;
		private Nullable<byte> _ReadBuffer;
		private SerialPort _Port;
		private byte _SequenceNumber;
		
		public event EventHandler<MessageReceviedEventArgs> MessageReceived;
		public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
		
		public Driver(String portName)
		{
			this.Port = new SerialPort(portName);
			this.WriteTimeout = 800;
			this.ReadTimeout = 800;
			this.BaudRate = 9600;			
			this.ReadBuffer = null;

		}

		protected ConnectionErrors LastError {
			set {
				this._LastError = value;
			}
			get {
				return this._LastError;
			}
		}

		/// <summary>
		/// Obtiene o establece el timeout de lectura.
		/// </summary>
		/// <value>
		/// Timeout de lectura.
		/// </value>
		public int ReadTimeout {
			set {
				this.Port.ReadTimeout = value;
			}
			get {
				return this.Port.ReadTimeout;
			}
		}

		/// <summary>
		/// Obtiene o establece el timeout de escritura.
		/// </summary>
		/// <value>
		/// Timeout de escritura.
		/// </value>
		public int WriteTimeout {
			set {
				this.Port.WriteTimeout = value;
			}
			get {
				return this.Port.WriteTimeout;
			}
		}

		/// <summary>
		/// Obtiene o establece la velocidad de conexion en baudios.
		/// </summary>
		/// <value>
		/// Velocidad de conexion en baudios.
		/// </value>
		public int BaudRate {
			set {
				this.Port.BaudRate = value;
			}
			get {
				return this.Port.BaudRate;
			}
		}

		/// <summary>
		/// Obtiene o establece el buffer de lectura de 1 byte para soportar la operacion PeekByte.
		/// </summary>
		/// <value>
		/// Buffer de lectura de 1 byte. Null si el buffer esta vacio.
		/// </value>
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

		/// <summary>
		/// Obtiene o establece el numero de secuencia actual para el intercambio de mensajes.
		/// </summary>
		/// <value>
		/// Numero de secuencia.
		/// </value>
		protected Byte SequenceNumber {
			set {
				this._SequenceNumber = value;
			}
			get {
				return this._SequenceNumber;
			}
		}
		
		public void Open()
		{
			lock (this) {
				if (!this.Port.IsOpen) {
					this.ReadBuffer = null;
					this.Port.Open();
					this.ResetSequenceNumber();
				}
			}
		}
		
		public void Close()
		{
			lock (this) {
				this.Port.Close();
			}
		}

		protected void ResetSequenceNumber()
		{
			Random rnd = new Random();
			byte last = this.SequenceNumber;

			while (last == this.SequenceNumber)
				this.SequenceNumber = (byte)rnd.Next(0x20,0x7F);
		}

		protected void IncrementSequenceNumber()
		{
			if (this.SequenceNumber == 0x7F) {
				this.ResetSequenceNumber();
			} else {
				this.SequenceNumber++;
			}
		}

		/// <summary>
		/// Realiza el envio no bloqueante de un frame de datos al dispositivo.
		/// La invocacion de este metodo devuelve el control cuando el dispositivo
		/// completa la operacion y devuelve un mensaje.
		/// Los datos a enviar no deben poseer registros de encabezado, solo de datos.
		/// </summary>
		/// <returns>
		/// El mensaje devuelto por el dispositivo.
		/// </returns>
		/// <param name='data'>
		/// Frame de datos a enviar al dispositivo.
		/// </param>
		public void Send(byte[] data)
		{
			Thread t = new Thread(delegate(object obj) {
				try {
					byte[] response = this.BlockSend((byte[])obj);
					this.OnMessageReceived(new MessageReceviedEventArgs(response));
				} catch (Exception) {
					//TODO: Reportar correctamente los errores para los casos asincronos.
					this.OnErrorReceived(new ErrorReceivedEventArgs(ConnectionErrors.TIMEOUT_ERROR));
				}
			}
			);
			t.Name = Environment.TickCount.ToString();
			t.Start(data);
		}

		/// <summary>
		/// Realiza el envio bloqueante de un frame de datos al dispositivo.
		/// La invocacion de este metodo devuelve el control cuando el dispositivo
		/// completa la operacion y devuelve un mensaje.
		/// Los datos a enviar no deben poseer registros de encabezado, solo de datos.
		/// </summary>
		/// <returns>
		/// El mensaje devuelto por el dispositivo.
		/// </returns>
		/// <param name='data'>
		/// Frame de datos a enviar al dispositivo.
		/// </param>
		public byte[] BlockSend(byte[] data)
		{

			lock (this) {

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


				return this.GetMessageFromResponseFrame(this.SendReceiveFrame(frame)).ToArray();
			}
		}

		/// <summary>
		/// Obtiene el mensaje a partir del frame de respuesta del dispositivo.
		/// </summary>
		/// <returns>
		/// El mensaje contenido en el frame de respuesta.
		/// </returns>
		/// <param name='frame'>
		/// Frame de respuesta.
		/// </param>
		protected List<byte> GetMessageFromResponseFrame(List<byte> frame)
		{
			return frame.GetRange(2, frame.Count - 7);
		}

		/// <summary>
		/// Envia un frame y espera la respuesta.
		/// </summary>
		/// <returns>
		/// Respuesta al frame enviado.
		/// </returns>
		protected List<byte> SendReceiveFrame(List<byte> frame)
		{
			int tries = 1;

			while (tries <= 4) {
				try {
					tries++;
					this.PortWrite(frame.ToArray(), 0, frame.Count);
					List<byte> response = this.ReceiveResponse();
					return response;
				} catch (NegativeAcknowledgeException) {
	
				} catch (Exception e) {
					throw e;
				}
			}

			throw new Exception("El mensaje no pudo ser enviado al dispositivo.");
		}


		/// <summary>
		/// Escribe en el puerto serie abierto para el driver.
		/// </summary>
		/// <param name='data'>
		/// Datos a escribir.
		/// </param>
		/// <param name='index'>
		/// Indice desde donde se empieza a escribir a partir del arreglo pasado como parametro.
		/// </param>
		/// <param name='count'>
		/// Cantidad de bytes a escribir.
		/// </param>
		protected void PortWrite(byte[] data, int index, int count)
		{
			try {
				this.Port.Write(data, index, count);
			} catch (TimeoutException e) {
				throw new TimeoutException(
					"Se ha agotado el tiempo de espera para escribir en el puerto.",
					e
				);
			} catch (Exception e) {
				throw e;
			}

		}

		/// <summary>
		/// Lee un byte desde el puerto. Primero verifica si existe informacion en el buffer de 1 byte.
		/// </summary>
		/// <returns>
		/// El byte leido.
		/// </returns>
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

		/// <summary>
		/// Funcionalidad PeekByte para verificar si hay un byte en el puerto sin leerlo.
		/// En realidad lo lee, pero lo deposita en un buffer.
		/// </summary>
		/// <returns>
		/// El proximo byte a leer.
		/// </returns>
		protected byte PortPeekByte()
		{			
			if (this.ReadBuffer == null) {
				while (this.ReadBuffer == null) {
					try {
						this.ReadBuffer = (byte)this.Port.ReadByte();
					} catch (TimeoutException) {
						Console.WriteLine("Timeout");
					}
				}
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
		
		protected void OnErrorReceived(ErrorReceivedEventArgs e)
		{
			if (this.ErrorReceived != null) {
				this.ErrorReceived(this, e);
			}
		}
		
		protected void OnMessageReceived(MessageReceviedEventArgs e)
		{
			if (this.MessageReceived != null) {
				this.MessageReceived(this, e);
			}
		}


		/// <summary>
		/// Lee un frame desde el puerto serie y valida que sea correcto.
		/// </summary>
		/// <returns>
		/// Frame leido.
		/// </returns>
		protected List<byte> ReadFrame()
		{
			byte code;
			List<byte> frame = new List<byte>(); 

			//STX
			code = this.PortReadByte();
			if (code != (byte)CommunicationCodes.STX) 
				throw new Exception("El formato del frame es incorrecto. Se esperaba STX.");
			frame.Add(code);

			//Sequence number
			code = this.PortReadByte();

			if (code != this.SequenceNumber) 
				throw new InvalidSequenceNumberException(
						this.SequenceNumber, 
						code
				);

			frame.Add(code);

			//Message body + ETX
			while ((code = this.PortReadByte()) != (byte)CommunicationCodes.ETX) {
				frame.Add(code);
			}

			frame.Add(code);

			//CheckSum
			for (int i=1; i<=4; i++) {
				code = this.PortReadByte();
				frame.Add(code);
			}
	


			Int16 checkSum = Int16.Parse(
				Encoding.ASCII.GetString(frame.GetRange(frame.Count - 4, 4).ToArray()),
				System.Globalization.NumberStyles.AllowHexSpecifier
			);

			//Comprobar checksum
			if (checkSum != this.GenerateCheckSum(frame.GetRange(0, frame.Count - 4).ToArray()))
				throw new InvalidChecksumException();

			return frame;

		}


		/// <summary>
		/// Recibe una respuesta desde la impresora fiscal.
		/// </summary>
		/// <returns>
		/// La respuesta.
		/// </returns>
		protected List<byte> ReceiveResponse()
		{
			List<byte> messageData = null;
			byte code;


	
			while (messageData == null) {

				try {
					code = (byte)this.PortPeekByte();
				} catch (Exception e) {
					throw e;
				}

				/*
				 * A esta altura el Timer del protocolo podria haberse agotado
				 * sin embargo, si al mismo tiempo el mensaje llego, se da prioridad
				 * al mensaje.
				 */

				switch (code) {
				case (byte)CommunicationCodes.STX:
					try {
						//Leer el frame
						messageData = this.ReadFrame();
					} catch (IOException e) {

						//Excepcion de IO. Levantar el error.
						throw e;
					} catch (InvalidSequenceNumberException) {

						//Seguir buscando el proximo frame
						messageData = null;
					} catch (InvalidChecksumException) {

						//Enviar NAK
						this.Port.Write(new Byte[] {(byte)CommunicationCodes.NAK}, 0, 1);
						messageData = null;
					} catch (Exception) {

						//Seguir buscando el proximo frame
						messageData = null;
					}
					break;
				case (byte)CommunicationCodes.DC2:
					this.PortReadByte();
						
					break;
				case (byte)CommunicationCodes.DC4:
					this.PortReadByte();
						
					break;
				case (byte)CommunicationCodes.NAK:
					//Negative Acknowledge recibido. Levantar excepcion.
					this.PortReadByte();
					throw new NegativeAcknowledgeException();
				default:
					this.PortReadByte();

					break;
				}

			}


			this.IncrementSequenceNumber();

			return messageData;
		}




	}
}

