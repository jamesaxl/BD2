using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamCloseRequestMessage), "Deserialize")]
	class TransparentStreamCloseRequestMessage : ObjectBusMessage
	{
		Guid id;

		public Guid Id {
			get {
				return id;
			}
		}

		Guid streamID;

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		public TransparentStreamCloseRequestMessage (Guid id, Guid streamID)
		{
			this.id = id;
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

