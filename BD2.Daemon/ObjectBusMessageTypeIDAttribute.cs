using System;

namespace BD2.Daemon
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ObjectBusMessageTypeIDAttribute : Attribute
	{
		Guid objectTypeID;

		public Guid ObjectTypeID {
			get {
				return new Guid (objectTypeID.ToByteArray ());
			}
		}

		public ObjectBusMessageTypeIDAttribute (string objectTypeID)
		{
			this.objectTypeID = Guid.Parse (objectTypeID);
		}
	}
}

