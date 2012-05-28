using System;
namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages
{
	public class Message
	{
		public EstadoImpresora EstadoImpresora;
		public EstadoFiscal EstadoFiscal;

		public Message()
		{
			this.EstadoFiscal = new EstadoFiscal();
			this.EstadoImpresora = new EstadoImpresora();
		}

		public override string ToString()
		{
			return 
@"
	Response Message
		EstadoFiscal:
			DocumentoFiscalAbierto: " + this.EstadoFiscal.DocumentoFiscalAbierto + @"
		EstadoImpresora:
			ImpresoraSinPapel: " + this.EstadoImpresora.ImpresoraSinPapel + @"
";
		}
	}
}

