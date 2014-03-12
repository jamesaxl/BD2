using System;

namespace BD2.Chunk
{
	public class ChunkData
	{
		byte[] id;

		public byte[] Id {
			get {
				return id;
			}
		}

		byte[][] dependencies;

		public byte[][] Dependencies {
			get {
				return dependencies;
			}
		}

		byte[] data;

		public byte[] Data {
			get {
				return data;
			}
		}

		public ChunkData (byte[] id, byte[][] dependencies, byte[] data)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			if (data == null)
				throw new ArgumentNullException ("data");
			this.id = id;
			for (int n = 0; n != dependencies.Length; n++) {
				if (dependencies [n] == null) {
					throw new ArgumentNullException (string.Format ("dependencies[{0}]", n));
				}
			}
			this.dependencies = dependencies;
			this.data = data;
		}
	}
}

