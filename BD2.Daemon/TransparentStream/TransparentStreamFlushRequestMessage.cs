using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamFlushRequestMessage), "Deserialize")]
	class TransparentStreamFlushRequestMessage : ObjectBusMessage
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

		public TransparentStreamFlushRequestMessage (Guid id, Guid streamID)
		{
			this.id = id;
			this.streamID = streamID;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
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