using System;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations;
using System.Collections.Generic;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets
{
	
	/// <summary>
	/// Operacion de impresora fiscal.
	/// Abrir Comprobante Ticket Fiscal.
	/// </summary>
	public class AbrirOperation : Operation
	{
		public Boolean RealizarDNFH;
			
		public AbrirOperation(Boolean realizarDNFH)
		{
			this.RealizarDNFH = realizarDNFH;
		}
		
		protected override byte Code
		{
			get {
				return 0x40;
			}
		}
				
		protected override List<byte> GenerateBytes()
		{
			List<byte> result = base.GenerateBytes();
			
			result.Add(0x1C);
			result.Add((byte)(this.RealizarDNFH?'G':'C'));
			
			return result;
		}

	}
}

