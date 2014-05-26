using System;

namespace BD2.Block.Daemon
{
	public abstract class ObjectBusMessage
	{
		public abstract Guid TypeID { get; }

		public abstract byte[] GetMessageBody ();
	}
}
