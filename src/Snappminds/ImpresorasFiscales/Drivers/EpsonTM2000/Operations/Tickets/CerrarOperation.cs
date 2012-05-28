using System;
using System.Collections.Generic;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets
{
	public class CerrarOperation : Operation
	{
		public Boolean CorteTotal;
			
		public CerrarOperation(Boolean corteTotal)
		{
			this.CorteTotal = corteTotal;
		}
		
		protected override byte Code
		{
			get {
				return 0x45;
			}
		}
				
		protected override List<byte> GenerateBytes()
		{
			List<byte> result = base.GenerateBytes();
			
			result.Add(0x1C);
			result.Add((byte)(this.CorteTotal?'T':'P'));
			
			return result;
		}

	}
}

