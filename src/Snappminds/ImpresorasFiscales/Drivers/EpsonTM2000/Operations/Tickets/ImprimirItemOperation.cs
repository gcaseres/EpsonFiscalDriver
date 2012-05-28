using System;
using System.Collections.Generic;
using System.Text;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations;
using System.Globalization;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets
{
	public enum CalificadorLineaItem
	{
		MontoAgregadoMercaderia = 0x4d,
		Reversion = 0x62,
		Bonificacion = 0x52,
		AnulaBonificacion = 0x72
	}
	
	public class ImprimirItemOperation : Operation
	{
		private String _TextoFiscal;
		public Decimal Cantidad;
		public Decimal Monto;
		public uint TasaImpositiva;
		public CalificadorLineaItem Calificador;
		private uint _Unidades;
		public uint TasaAjusteVariable;
		public Nullable<Decimal> ImpuestosInternosFijos;
		
		public ImprimirItemOperation(String textoFiscal, Decimal cantidad, Decimal monto, uint tasaImpositiva, CalificadorLineaItem calificador)
		{
			this.TextoFiscal = textoFiscal;
			this.Cantidad = cantidad;
			this.Monto = monto;
			this.TasaImpositiva = tasaImpositiva;
			this.Calificador = calificador;
		}
		
		public String TextoFiscal
		{
			set {
				if (value.Length > 20)
					throw new ArgumentOutOfRangeException("El texto fiscal no puede tener mas de 20 caracteres.");
				
				this._TextoFiscal = value;
			}
			get {
				return this._TextoFiscal;
			}
		}
		
		public uint Unidades
		{
			set {
				if (value > 99999)
					throw new ArgumentOutOfRangeException("La cantidad de unidades no puede exceder las 99999.");
				
				this._Unidades = value;
			}
			get {
				return this._Unidades;
			}
		}
	
		protected override byte Code {
			get {
				return 0x42;
			}
		}
		

		
		protected override List<byte> GenerateBytes()
		{
			
			List<byte> result = base.GenerateBytes();
			
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.TextoFiscal));
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.Cantidad.ToString("00000.000", CultureInfo.InvariantCulture.NumberFormat).Replace(".","")));
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.Monto.ToString("0000000.00", CultureInfo.InvariantCulture.NumberFormat).Replace(".","")));
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.TasaImpositiva.ToString("0000")));
			result.Add(0x1C);
			result.Add((byte)this.Calificador);
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.Unidades.ToString("00000")));
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.TasaAjusteVariable.ToString("00000000")));
			result.Add(0x1C);
			if (this.ImpuestosInternosFijos.HasValue)
				result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.ImpuestosInternosFijos.Value.ToString("000000000.00000000", CultureInfo.InvariantCulture.NumberFormat).Replace(".","")));
			
			
			
			return result;
		}
	}
}

