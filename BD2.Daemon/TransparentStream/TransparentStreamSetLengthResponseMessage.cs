using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamSetLengthResponseMessage), "Deserialize")]
	class TransparentStreamSetLengthResponseMessage : ObjectBusMessage
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

		public TransparentStreamSetLengthResponseMessage (Guid requestID, Exception exception)
		{
			this.requestID = requestID;
			this.exception = exception;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				MS.Write (requestID.ToByteArray (), 0, 16);
				if (exception == null) {
					MS.WriteByte (0);
				} else {
					MS.WriteByte (1);
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BF = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
					BF.Serialize (MS, exception);
				}
				return MS.GetBuffer ();
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

