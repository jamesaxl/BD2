using System;
using System.IO;
using System.Collections.Generic;
using BSO;
using BD2.Common;

namespace BD2.Repo.Memory
{
	public class Repository : ChunkRepository
	{
		SortedDictionary<byte[], byte[]> Items = new SortedDictionary<byte[], byte[]> ();
		SortedDictionary<byte[], SortedSet<byte[]>> Itemdependencies = new SortedDictionary<byte[], SortedSet<byte[]>> ();
		Guid id = Guid.NewGuid ();

		public override Guid ID {
			get {
				return id;
			}	
		}

		#region implemented abstract members of ChunkRepository

		public override void Push (byte[] ChunkID, byte[] Data)
		{
			Items.Add (ChunkID, Data);
		}

		public override byte[] Pull (byte[] ChunkID)
		{
			return Items [ChunkID];
		}

		public override IEnumerator<KeyValuePair<byte[], byte[]>> Enumerate ()
		{
			return Items.GetEnumerator ();
		}

		public override SortedSet<byte[]> GetIndependentChunks ()
		{
			throw new NotImplementedException ();
		}

		public override int GetLeastCost (int CurrentMinimum, byte[] ChunkDescriptor)
		{
			throw new NotImplementedException ();
		}

		public override int GetMaxCostForAny ()
		{
			throw new NotImplementedException ();
		}

		public override byte[][] GetDependencies (byte[] ChunkID)
		{
			throw new NotImplementedException ();
		}

		#endregion

	}
}
