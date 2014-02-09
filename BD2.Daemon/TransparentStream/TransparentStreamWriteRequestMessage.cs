using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamWriteRequestMessage), "Deserialize")]
	class TransparentStreamWriteRequestMessage : ObjectBusMessage
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

		byte[] buffer;

		public byte[] Buffer {
			get {
				return buffer;
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

		public TransparentStreamWriteRequestMessage (Guid id, Guid streamID, byte[] buffer, int offset, int count)
		{
			this.id = id;
			this.streamID = streamID;
			this.buffer = buffer;
			this.offset = offset;
			this.count = count;
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

