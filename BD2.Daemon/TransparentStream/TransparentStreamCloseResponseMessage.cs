using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamCloseResponseMessage), "Deserialize")]
	class TransparentStreamCloseResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		Exception exception;

		public Exception Exception {
			get {
				return exception;
			}
		}

		public TransparentStreamCloseResponseMessage (Guid requestID, Exception exception)
		{
			this.requestID = requestID;
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
