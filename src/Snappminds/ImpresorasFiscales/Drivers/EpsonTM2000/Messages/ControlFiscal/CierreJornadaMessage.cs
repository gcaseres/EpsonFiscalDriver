using System;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Messages;


namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages.ControlFiscal
{
	public class CierreJornadaMessage : Message
	{
		public UInt16 Numero;
		public UInt16 DocumentosFiscalesCancelados;
		public UInt16 DocumentosNoFiscalesHomologados;
		public UInt16 DocumentosNoFiscalesNoHomologados;
		public UInt16 ComprobantesFiscalesTicketFacturaBCEmitidos;
		public UInt16 ComprobantesFacturaAEmitidos;
		public UInt32 UltimaNumeracionTicketFacturaBCEmitida;
		public Decimal MontoTotalFacturado;
		public Decimal MontoTotalIVACobrado;
		public Decimal ImporteTotalPercepciones;
		public UInt32 UltimaNumeracionFacturaA;

		public CierreJornadaMessage()
		{
		}

		public override string ToString()
		{
			return base.ToString() + 
@"	CierreJornada:
		Numero: " + this.Numero + @"
		Documentos Fiscales Cancelados: " + this.DocumentosFiscalesCancelados + @"
		Documentos No Fiscales Homologados Emitidos: " + this.DocumentosNoFiscalesHomologados + @"
		Documentos No Fiscales No Homologados Emitidos: " + this.DocumentosNoFiscalesNoHomologados + @"
		Comprobantes Fiscales Ticket, Factura B,C o Ticket-Factura B,C Emitidos: " + this.ComprobantesFiscalesTicketFacturaBCEmitidos + @"
		Comprobantes Ticket-Factura A y Factura A emitidos: " + this.ComprobantesFacturaAEmitidos + @"
		Numeracion ultimo comprobante Ticket, Ticket-Factura B,C, Factura B,C emitido: " + this.UltimaNumeracionTicketFacturaBCEmitida + @"
		Monto Total Facturado: " + this.MontoTotalFacturado + @"
		Monto Total IVA Cobrado: " + this.MontoTotalIVACobrado + @"
		Importe Total Percepciones: " + this.ImporteTotalPercepciones + @"
		Numeracion ultimo comprobante Ticket-Factura, Factura A emitido: " + this.UltimaNumeracionFacturaA + @"
";
		}
	}
}

