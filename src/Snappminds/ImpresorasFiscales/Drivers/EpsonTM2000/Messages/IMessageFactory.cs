using System;
namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages
{
	public interface IMessageFactory
	{
		Message CreateFromBytes(byte[] data);
	}
}

