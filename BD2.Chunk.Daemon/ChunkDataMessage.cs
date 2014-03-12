using System;
using System.Collections.Generic;

namespace BD2.Chunk.Daemon
{
	public class ChunkDataMessage : BD2.Daemon.ObjectBusMessage
	{
		List<ChunkData> chunkData;

		public ChunkDataMessage ()
		{

		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (chunkData.Length);
					for (int n = 0; n != chunkData.Length; n++) {
					
					}
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

