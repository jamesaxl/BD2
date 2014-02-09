using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamGetWriteTimeoutResponseMessage), "Deserialize")]
	class TransparentStreamGetWriteTimeoutResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		int writeTimeout;

		public int WriteTimeout {
			get {
				return writeTimeout;
			}
		}

		public TransparentStreamGetWriteTimeoutResponseMessage (Guid requestID, int writeTimeout)
		{
			this.requestID = requestID;
			this.writeTimeout = writeTimeout;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (requestID.ToByteArray ());
					BW.Write (writeTimeout);
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("");
			}
		}
		#endregion
	}
}

