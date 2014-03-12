using System;
using BD2.Daemon;

namespace BD2.Chunk.Daemon
{
	[ObjectBusMessageTypeIDAttribute("556709a3-fe93-413e-ad51-5e72d45468cd")]
	[ObjectBusMessageDeserializerAttribute(typeof(RequestChunkRanges), "Deserialize")]
	public class RequestChunkRanges : ObjectBusMessage
	{
		Tuple<byte[], byte[]>[] ranges;

		public RequestChunkRanges (System.Collections.Generic.IEnumerable<Tuple<byte[], byte[]>> ranges)
		{
			this.ranges = (new System.Collections.Generic.List<Tuple<byte[], byte[]>> (ranges)).ToArray ();
		}

		public static RequestChunkRanges Deserialize (byte[] bytes)
		{
			Tuple<byte[],byte[]>[] ranges;
			using (System.IO.MemoryStream MS  = new System.IO.MemoryStream (bytes,false)) {
				using (System.IO.BinaryReader BR= new System.IO.BinaryReader(MS)) {
					ranges = new Tuple<byte[], byte[]>[BR.ReadInt32 ()];
					for (int n = 0; n != ranges.Length; n++) {
						ranges [n] = new Tuple<byte[], byte[]> (BR.ReadBytes (BR.ReadInt32 ()), BR.ReadBytes (BR.ReadInt32 ()));
					}
				}
				return new RequestChunkRanges (ranges);
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (ranges.Length);
					for (int n = 0; n != ranges.Length; n++) {
						BW.Write (ranges [n].Item1.Length);
						BW.Write (ranges [n].Item1);
						BW.Write (ranges [n].Item2.Length);
						BW.Write (ranges [n].Item2);
					}
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("556709a3-fe93-413e-ad51-5e72d45468cd");
			}
		}
		#endregion
	}
}
