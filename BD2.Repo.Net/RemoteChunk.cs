using System;
using BSO;
using System.IO;

namespace BD2
{
	public class RemoteChunk : ChunkDescriptor
	{
		string path;

		public string Path {
			get {
				return path;
			}
		}

		Peer repo;

		public RemoteChunk (Peer Repo, long PrivID)
		{
			if (Repo == null)
				throw new ArgumentNullException ("Repo");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			repo = Repo;
			path = Path;
			mPrivID = PrivID;
		}

		byte[] data;

		public byte[] Data {
			get {
				if (data == null) {
					byte[] Bytes;
					if (wData == null) {
						Bytes = System.IO.File.ReadAllBytes (path);
						data = Bytes;
						hash = null;
						return data;
					} else {
						Bytes = (byte[])wData.Target;
						if (Bytes == null) {
							Bytes = System.IO.File.ReadAllBytes (path);
							wData.Target = Bytes;
						}
						return Bytes;
					}
				}
				return data;
			} 
		}

		WeakReference wData;

		public override void GoConservative ()
		{
			if (data != null) {
				wData = new WeakReference (data);
				data = null;
			}
		}

		public override void GoLive ()
		{
			data = Data;
			wData = null;
		}

		public RemoteChunk (Peer Repo, long PrivID, string Path, byte[] Data)
		{
			data = Data;
			repo = Repo;
			path = Path;
			mPrivID = PrivID;
			//sync
			System.IO.File.WriteAllBytes (Path, Data);
		}

		public override long Length {
			get {
				return data.Length;
			}
		}

		byte[] hash;

		public override byte[] Hash {
			get {
				if (hash == null)
					hash = Data.SHA1 ();
				return (byte[])hash.Clone ();
			}
		}

		public override Stream GetData ()
		{
			bool L = false;
			return new ParallelPagedEnumerator<Tuple<int, byte[]>> (Data, (r) => {
				if (L == false) {
					L = true;
					return new Tuple<int, Byte[]> (0, (byte[])r);
				}
				return null;
			}, 8, 2);
		}

		private long mPrivID;

		public override long PrivID { get { return mPrivID; } }
	}
}

