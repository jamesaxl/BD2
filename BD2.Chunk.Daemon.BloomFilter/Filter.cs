using System;
using BD2.Chunk.Daemon;

namespace BD2.BloomFilter
{
	public class Filter : IRangedFilter
	{
		byte[] first;
		byte[] last;

		public Filter (byte[] first, byte[] last)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (last == null)
				throw new ArgumentNullException ("last");
			this.first = first;
			this.last = last;
		}

		public float Contains (byte[] chunkID)
		{
			throw new NotImplementedException ();
		}

		public byte[] GetMessageBody ()
		{
			throw new NotImplementedException ();
		}

		public string FilterTypeName {
			get {
				return "Bloom";
			}
		}

		public byte[] FirstChunk {
			get {
				return first;
			}
		}

		public byte[] LastChunk {
			get {
				return last;
			}
		}

		public int CompareTo (object obj)
		{
			throw new NotImplementedException ();
		}
	}
}

