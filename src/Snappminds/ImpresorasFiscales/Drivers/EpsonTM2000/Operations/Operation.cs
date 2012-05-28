using System;
using System.Collections.Generic;

namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Operations
{
	public abstract class Operation : Snappminds.ImpresorasFiscales.Operations.Operation
	{		
		
		public Operation()
		{
		}
		
		protected abstract byte Code
		{
			get;
		}
		
		/// <summary>
		/// Obtiene la operacion en formato de bytes para ser enviada
		/// a la impresora fiscal.
		/// Para modificar la cadena de bytes se recomienda redefinir <see cref="Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Operation.GenerateBytes"/>
		/// </summary>
		/// <returns>
		/// Cadena de bytes para ser enviada a la impresora fiscal.
		/// </returns>
		public byte[] GetBytes()
		{
			return this.GenerateBytes().ToArray();
		}
		
		/// <summary cref="Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Operation.GenerateBytes ">
		/// Generates the bytes.
		/// </summary>
		/// <returns>
		/// The bytes.
		/// </returns>
		protected virtual List<Byte> GenerateBytes()
		{
			List<Byte> result = new List<Byte>();
			result.Add(this.Code);
			
			return result;
		}
	}
}

