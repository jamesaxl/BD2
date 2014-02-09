using System;
using System.Collections.Generic;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamReadRequestMessage), "Deserialize")]
	class TransparentStreamReadRequestMessage : ObjectBusMessage
	{
		Guid id;

		public Guid Id {
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

		int offset;

		public int Offset {
			get {
				return offset;
			}
		}

		int count;

		public int Count {
			get {
				return count;
			}
		}

		public TransparentStreamReadRequestMessage (Guid id, Guid streamID, int offset, int count)
		{
			this.id = id;
			this.streamID = streamID;
			this.offset = offset;
			this.count = count;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
					BW.Write (offset);
					BW.Write (count);
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}

