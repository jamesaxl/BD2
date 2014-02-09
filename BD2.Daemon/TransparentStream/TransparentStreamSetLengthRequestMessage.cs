using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamSetLengthRequestMessage), "Deserialize")]
	class TransparentStreamSetLengthRequestMessage : ObjectBusMessage
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

		long length;

		public long Length {
			get {
				return length;
			}
		}

		public TransparentStreamSetLengthRequestMessage (Guid id, Guid streamID, long length)
		{
			this.id = id;
			this.streamID = streamID;
			this.length = length;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
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

