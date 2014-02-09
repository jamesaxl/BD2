using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamGetReadTimeoutResponseMessage), "Deserialize")]
	class TransparentStreamGetReadTimeoutResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		int readTimeout;

		public int ReadTimeout {
			get {
				return readTimeout;
			}
		}

		public TransparentStreamGetReadTimeoutResponseMessage (Guid requestID, int readTimeout)
		{
			this.requestID = requestID;
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

