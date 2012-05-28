using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages
{
	/// <summary>
	/// Factory para mensajes abstractos.
	/// </summary>
	public class MessageFactory : IMessageFactory
	{
		public MessageFactory()
		{
		}

		public virtual Message CreateFromBytes(byte[] data)
		{
			Message message = new Message();
			this.InitializeMessage(message, data);

			return message;
		}

		/// <summary>
		/// Inicializa un mensaje.
		/// </summary>
		/// <param name='message'>
		/// Mensaje a inicializar.
		/// </param>
		protected virtual void InitializeMessage(Message message, byte[] frameData)
		{
			List<byte> data = new List<byte>(frameData);

			String hexString;


			hexString = ASCIIEncoding.ASCII.GetString(data.GetRange(2,4).ToArray());
			BitArray bitsEstadoImpresora = new BitArray(BitConverter.GetBytes(Convert.ToUInt16(hexString,16)));
			message.EstadoImpresora.FallaDeImpresora = bitsEstadoImpresora[2];
			message.EstadoImpresora.ImpresoraFueraDeLinea = bitsEstadoImpresora[3];
			message.EstadoImpresora.PocoPapelCintaAuditoria = bitsEstadoImpresora[4];
			message.EstadoImpresora.PocoPapelComprobantes = bitsEstadoImpresora[5];
			message.EstadoImpresora.BufferImpresoraLleno = bitsEstadoImpresora[6];
			message.EstadoImpresora.BufferImpresoraVacio = bitsEstadoImpresora[7];
			message.EstadoImpresora.EntradaHojasSueltasFrontalPreparada = bitsEstadoImpresora[8];
			message.EstadoImpresora.HojaSueltaFrontalPreparada = bitsEstadoImpresora[9];
			message.EstadoImpresora.TomaDeHojasParaValidacionPreparada = bitsEstadoImpresora[10];
			message.EstadoImpresora.PapelParaValidacionPresente = bitsEstadoImpresora[11];
			message.EstadoImpresora.CajonDeDineroAbierto = bitsEstadoImpresora[12];
			message.EstadoImpresora.ImpresoraSinPapel = bitsEstadoImpresora[14];
			message.EstadoImpresora.Error = bitsEstadoImpresora[15];

			hexString = ASCIIEncoding.ASCII.GetString(data.GetRange(7,4).ToArray());
			BitArray bitsEstadoFiscal = new BitArray(BitConverter.GetBytes(Convert.ToUInt16(hexString,16)));
			message.EstadoFiscal.ErrorComprobacionMemoriaFiscal = bitsEstadoFiscal[0];
			message.EstadoFiscal.ErrorComprobacionMemoriaTrabajo = bitsEstadoFiscal[1];
			message.EstadoFiscal.PocaBateria = bitsEstadoFiscal[2];
			message.EstadoFiscal.ComandoNoReconocido = bitsEstadoFiscal[3];
			message.EstadoFiscal.CampoDeDatosInvalido = bitsEstadoFiscal[4];
			message.EstadoFiscal.ComandoNoValidoParaEstadoFiscal = bitsEstadoFiscal[5];
			message.EstadoFiscal.DesbordamientoDeTotales = bitsEstadoFiscal[6];
			message.EstadoFiscal.MemoriaFiscalLlena = bitsEstadoFiscal[7];
			message.EstadoFiscal.MemoriaFiscalCasiLlena = bitsEstadoFiscal[8];
			message.EstadoFiscal.ImpresorFiscalCertificado = bitsEstadoFiscal[9];
			message.EstadoFiscal.ImpresorFiscalFiscalizado = bitsEstadoFiscal[10];
			message.EstadoFiscal.NecesitaCierreTicketOJornadaFiscal = bitsEstadoFiscal[11];
			message.EstadoFiscal.DocumentoFiscalAbierto = bitsEstadoFiscal[12];
			message.EstadoFiscal.DocumentoAbiertoEnRolloPapel = bitsEstadoFiscal[13];
			message.EstadoFiscal.ImpresionEnHojaSueltaInicializada = bitsEstadoFiscal[14];
			message.EstadoFiscal.Error = bitsEstadoFiscal[15];
		}

	}
}

