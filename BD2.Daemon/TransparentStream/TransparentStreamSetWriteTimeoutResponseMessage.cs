using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamSetWriteTimeoutResponseMessage), "Deserialize")]
	class TransparentStreamSetWriteTimeoutResponseMessage : ObjectBusMessage
	{
		Guid streamID;

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		Exception exception;

		public Exception Exception {
			get {
				return exception;
			}
		}

		public TransparentStreamSetWriteTimeoutResponseMessage (Guid streamID, Exception exception)
		{
			this.streamID = streamID;
			this.exception = exception;
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

