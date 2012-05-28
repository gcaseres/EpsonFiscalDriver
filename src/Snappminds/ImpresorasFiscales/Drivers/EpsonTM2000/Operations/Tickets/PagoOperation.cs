using System;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets
{
	
	public enum CalificadorPago
	{
		CancelarComprobante = 0x43,
		SumaImportePagado = 0x54,
		AnulaPago = 0x74,
		DescuentoGlobal = 0x44,
		RecargoGlobal = 0x52		
	}
	
	/// <summary>
	/// Operacion de impresora fiscal.
	/// Generar un pago/descuento/recargo en ticket.
	/// </summary>
	public class PagoOperation : Operation
	{
		
		private String _TextoFiscal;
		public Decimal Monto;
		public CalificadorPago Calificador;
			
		public PagoOperation(String textoFiscal, Decimal monto, CalificadorPago calificador)
		{
			this.TextoFiscal= textoFiscal;
			this.Monto = monto;
			this.Calificador = calificador;
		}
		
		
		public String TextoFiscal
		{
			set {
				if (value.Length > 25)
					throw new ArgumentOutOfRangeException("El texto fiscal no puede tener mas de 25 caracteres.");
				
				this._TextoFiscal = value;
			}
			get {
				return this._TextoFiscal;
			}
		}
		
		protected override byte Code
		{
			get {
				return 0x44;
			}
		}
				
		protected override List<byte> GenerateBytes()
		{
			List<byte> result = base.GenerateBytes();
			
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.TextoFiscal));
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.Monto.ToString("0000000.00", CultureInfo.InvariantCulture.NumberFormat).Replace(".","")));
			result.Add(0x1C);
			result.Add((byte)this.Calificador);
			
			return result;
		}

	}
}

