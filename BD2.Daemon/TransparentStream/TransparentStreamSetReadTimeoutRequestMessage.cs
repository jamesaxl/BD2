using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamSetReadTimeoutRequestMessage), "Deserialize")]
	class TransparentStreamSetReadTimeoutRequestMessage : ObjectBusMessage
	{
		Guid streamID;

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		int readTimeout;

		public int ReadTimeout {
			get {
				return readTimeout;
			}
		}

		public TransparentStreamSetReadTimeoutRequestMessage (Guid streamID, int readTimeout)
		{
			this.streamID = streamID;
			this.readTimeout = readTimeout;
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

