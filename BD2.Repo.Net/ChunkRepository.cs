/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the bd2 nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;
using System.IO;
using System.Collections.Generic;
using BD2.Chunk;

namespace BD2.Repo.Net
{
	public class Repository : ChunkRepository
	{
		Stream peer;

		public Repository (Stream peer)
		{
			this.peer = peer;
		}

		static byte[] DependenciesToArray (byte[][] dependencies)
		{
			if (dependencies == null)
				return null;
			int lenOfDependencies = 0;
			for (int n = 0; n != dependencies.Length; n++) {
				lenOfDependencies += dependencies [n].Length + sizeof(int);
			}
			byte[] metadata = new byte[sizeof(int) + lenOfDependencies];
			MemoryStream metastream = new MemoryStream (metadata, true);
			BinaryWriter metawriter = new BinaryWriter (metastream);
			metawriter.Write (dependencies.Length);
			for (int n = 0; n != dependencies.Length; n++) {
				metawriter.Write (dependencies [n].Length);
				metawriter.Write (dependencies [n]);
			}
			return metadata;
		}

		static byte[][] ArrayToDependencies (byte[] array)
		{
			if (array == null)
				return null;
			MemoryStream metastream = new MemoryStream (array);
			BinaryReader metareader = new BinaryReader (metastream);
			int countOfDependencies = metareader.ReadInt32 ();
			byte[][] dependencies = new byte[countOfDependencies][];
			for (int n = 0; n != countOfDependencies; n++) {
				dependencies [n] = metareader.ReadBytes (metareader.ReadInt32 ());
			}
			return dependencies;
		}
		#region implemented abstract members of ChunkRepository
		public override void Push (byte[] chunkId, byte[] data, byte[][] dependencies)
		{
			throw new NotImplementedException ();
		}

		public override byte[] PullData (byte[] chunkID)
		{
			throw new NotImplementedException ();
		}

		public override byte[][] PullDependencies (byte[] chunkID)
		{
			throw new NotImplementedException ();
		}

		public override void Pull (byte[] chunkID, out byte[] data, out byte[][] dependencies)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<byte[]> Enumerate ()
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<byte[]> EnumerateTopLevels ()
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateDependencies ()
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<Tuple<byte[], byte[][]>> EnumerateTopLevelDependencies ()
		{
			throw new NotImplementedException ();
		}

		public override int GetLeastCost (int currentMinimum, byte[] chunkID)
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
