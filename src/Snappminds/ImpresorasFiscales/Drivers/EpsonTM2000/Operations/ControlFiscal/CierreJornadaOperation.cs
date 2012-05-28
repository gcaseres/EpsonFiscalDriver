using System;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations;
using System.Collections.Generic;


namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.ControlFiscal
{
	public enum TipoCierreJornada { CierreZ = 0x5A, CierreX = 0x58 }

	public class CierreJornadaOperation : Operation
	{
		public TipoCierreJornada TipoCierre;
		public Boolean ImprimirReporte;

		public CierreJornadaOperation(TipoCierreJornada tipoCierre, Boolean imprimirReporte)
		{
			this.TipoCierre = tipoCierre;
			this.ImprimirReporte = imprimirReporte;
		}

		protected override byte Code {
			get {
				return 0x39;
			}
		}

		protected override System.Collections.Generic.List<byte> GenerateBytes()
		{
			List<byte> result = base.GenerateBytes();
			
			result.Add(0x1C);
			result.Add((byte)this.TipoCierre);
			result.Add(0x1C);
			result.Add((byte)(this.ImprimirReporte?0x50:0x00));

			return result;
		}
	}
}

