using System;
using System.IO;
using System.Collections.Generic;
using BSO;
using BD2.Common;

namespace BD2.Repo.LevelDB
{
	public class Repository : ChunkRepository
	{
		public Repository (string Path)
		{

		}
		#region implemented abstract members of ChunkRepository
		#region implemented abstract members of ChunkRepository
		public override IEnumerable<byte[]> Enumerate ()
		{
			throw new NotImplementedException ();
		}
		#endregion
		public override void Push (byte[] ChunkID, byte[] Data)
		{
			throw new NotImplementedException ();
		}

		public override byte[] Pull (byte[] ChunkID)
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

		public override Guid ID {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}
