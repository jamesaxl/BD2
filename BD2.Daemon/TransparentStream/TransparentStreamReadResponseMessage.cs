using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamReadResponseMessage), "Deserialize")]
	class TransparentStreamReadResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		byte[] data;

		public byte[] Data {
			get {
				return data;
			}
		}

		public TransparentStreamReadResponseMessage (Guid requestID, byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			this.requestID = requestID;
			this.data = data;
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

