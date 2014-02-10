using System;

namespace BD2.Daemon
{
	public abstract class TransparentStreamMessageBase : ObjectBusMessage
	{
		public abstract Guid StreamID { get; }
	}
}

