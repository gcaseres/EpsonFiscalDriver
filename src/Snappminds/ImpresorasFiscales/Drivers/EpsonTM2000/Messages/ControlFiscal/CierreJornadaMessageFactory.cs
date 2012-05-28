using System;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Messages;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages.ControlFiscal
{
	public class CierreJornadaMessageFactory : MessageFactory
	{
		public CierreJornadaMessageFactory()
		{
		}

		public override Message CreateFromBytes(byte[] frameData)
		{
			CierreJornadaMessage message = new CierreJornadaMessage();
			this.InitializeMessage(message, frameData);

			List<byte> data = new List<byte>(frameData);
			String strValor;


			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(12,5).ToArray());
			message.Numero = Convert.ToUInt16(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(18,5).ToArray());
			message.DocumentosFiscalesCancelados = Convert.ToUInt16(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(24,5).ToArray());
			message.DocumentosNoFiscalesHomologados = Convert.ToUInt16(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(30,5).ToArray());
			message.DocumentosNoFiscalesNoHomologados = Convert.ToUInt16(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(36,5).ToArray());
			message.ComprobantesFiscalesTicketFacturaBCEmitidos = Convert.ToUInt16(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(42,5).ToArray());
			message.ComprobantesFacturaAEmitidos = Convert.ToUInt16(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(48,8).ToArray());
			message.UltimaNumeracionTicketFacturaBCEmitida = Convert.ToUInt32(strValor);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(57,12).ToArray());
			strValor += "." + ASCIIEncoding.ASCII.GetString(data.GetRange(69,2).ToArray());
			message.MontoTotalFacturado = Convert.ToDecimal(strValor, CultureInfo.InvariantCulture.NumberFormat);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(72,12).ToArray());
			strValor += "." + ASCIIEncoding.ASCII.GetString(data.GetRange(84,2).ToArray());
			message.MontoTotalIVACobrado = Convert.ToDecimal(strValor, CultureInfo.InvariantCulture.NumberFormat);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(87,12).ToArray());
			strValor += "." + ASCIIEncoding.ASCII.GetString(data.GetRange(99,2).ToArray());
			message.ImporteTotalPercepciones = Convert.ToDecimal(strValor, CultureInfo.InvariantCulture.NumberFormat);

			strValor = ASCIIEncoding.ASCII.GetString(data.GetRange(102,8).ToArray());
			message.UltimaNumeracionFacturaA = Convert.ToUInt32(strValor);

			return message;
		}
	}
}

