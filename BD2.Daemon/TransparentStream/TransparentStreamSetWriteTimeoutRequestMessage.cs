using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamSetWriteTimeoutRequestMessage), "Deserialize")]
	class TransparentStreamSetWriteTimeoutRequestMessage : ObjectBusMessage
	{
		Guid id;

		public Guid ID {
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

		int writeTimeout;

		public int WriteTimeout {
			get {
				return writeTimeout;
			}
		}

		public TransparentStreamSetWriteTimeoutRequestMessage (Guid id, Guid streamID, int writeTimeout)
		{
			this.id = id;
			this.streamID = streamID;
			this.writeTimeout = writeTimeout;
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

