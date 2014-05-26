using System;

namespace BD2.Block.Daemon
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ObjectBusMessageDeserializerAttribute : Attribute
	{
		Func<byte[], ObjectBusMessage> func;

		public ObjectBusMessageDeserializerAttribute (Func<byte[], ObjectBusMessage> func)
		{
			if (func == null)
				throw new ArgumentNullException ("func");
			this.func = func;
		}
	}
}

