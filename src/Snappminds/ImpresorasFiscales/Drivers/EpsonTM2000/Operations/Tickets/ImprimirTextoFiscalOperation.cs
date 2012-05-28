using System;
using System.Collections.Generic;
using System.Text;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets
{
	public class ImprimirTextoFiscalOperation : Operation
	{
		public String TextoFiscal;
		
		public ImprimirTextoFiscalOperation(String textoFiscal)
		{
			this.TextoFiscal = textoFiscal;
		}
	
		protected override byte Code {
			get {
				return 0x41;
			}
		}
		
		protected override List<byte> GenerateBytes()
		{
			List<byte> result = base.GenerateBytes();
			
			result.Add(0x1C);
			result.AddRange(ASCIIEncoding.ASCII.GetBytes(this.TextoFiscal));
			
			return result;
		}
	}
}

