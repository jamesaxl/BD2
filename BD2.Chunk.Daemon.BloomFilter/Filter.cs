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
		}
		#region IRangedFilter implementation
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
				throw new NotImplementedException ();
			}
		}

		public byte[] FirstChunk {
			get {
				throw new NotImplementedException ();
			}
		}

		public byte[] LastChunk {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
		#region IComparable implementation
		public int CompareTo (object obj)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}

