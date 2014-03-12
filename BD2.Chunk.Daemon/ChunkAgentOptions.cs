using System;
using System.Collections;
using System.Collections.Generic;

namespace BD2.Chunk.Daemon
{
	public class ChunkAgentOptions
	{
		ChunkRepository repository;

		public ChunkRepository Repository {
			get {
				return repository;
			}
		}

		public ChunkAgentOptions (ChunkRepository repository)
		{
			if (repository == null)
				throw new ArgumentNullException ("repository");
			this.repository = repository;
		}
	}
}

