using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamMessage), "Deserialize")]
	class TransparentStreamMessage : ObjectBusMessage
	{
		Guid streamID;

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		public TransparentStreamMessage (Guid streamID)
		{
			this.streamID = streamID;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			throw new NotImplementedException ();
		}

		public override Guid TypeID {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}

