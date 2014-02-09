using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamGetLengthResponseMessage), "Deserialize")]
	class TransparentStreamGetLengthResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequsetID {
			get {
				return requestID;
			}
		}

		long length;

		public long Length {
			get {
				return length;
			}
		}

		public TransparentStreamGetLengthResponseMessage (Guid requestID, long length)
		{
			this.requestID = requestID;
			this.length = length;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (requestID.ToByteArray ());
					BW.Write (length);
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

