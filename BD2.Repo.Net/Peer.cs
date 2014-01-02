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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
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
using BSO;
using System.Net.Sockets;

namespace BD2
{
	public class Peer : ChunkDataRepository
	{
		SortedDictionary<long, RemoteChunk> Files = new SortedDictionary<long, RemoteChunk> ();
		Socket socket;
		string path;
		ChunkDataRepository cache;

		public Peer (ChunkDataRepository Cache, Socket Socket)
		{
			cache = Cache;
			socket = Socket;
		}

		void ReadFiles ()
		{
			lock (Files) {
				using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("SELECT ID, Name FROM Files ORDER BY ID", Base)) {
					using (Mono.Data.Sqlite.SqliteDataReader DataReader = Comm.ExecuteReader()) {
						while (DataReader.Read()) {
							long ID = DataReader.GetInt64 (0);
							string Name = DataReader.GetString (1);
							this.Files.Add (ID, new RemoteChunk (this, ID, path + System.IO.Path.DirectorySeparatorChar + Name));
						}
					}
				}
			}
		}
		#region implemented abstract members of BD2.ObjectRepository
		public override ChunkDescriptor Push (ChunkDescriptor Chunk)
		{
			long ID = Files.Count + 1;
			string NName = Chunk.Hash.ToHexadecimal ();
			string FPath = path + System.IO.Path.DirectorySeparatorChar + NName;
			try {
				Byte[] Bytes = Chunk.GetPages ().ToByteArray ();
				RemoteChunk R = new RemoteChunk (this, ID, FPath, Bytes);
				lock (Files) {
					Files.Add (ID, R);
					using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("INSERT INTO Files (ID, Name, Hash, Attributes) VALUES (@p0,@p1,@p2,@p3)", Base)) {
						Comm.Parameters.AddWithValue ("p0", ID);
						Comm.Parameters.AddWithValue ("p1", NName);
						Comm.Parameters.AddWithValue ("p2", Chunk.Hash);// R.Hash = Chunk.Hash;
						Comm.Parameters.AddWithValue ("p3", null);
						Comm.ExecuteNonQuery ();
					}
				}
				return R;
			} catch (Exception ex) {
				if (NName != null) {
					if (System.IO.File.Exists (FPath)) {
						System.IO.File.Delete (FPath);
						throw new SystemException ("Cannot save chunk, Name:" + NName, ex);
					}
				}
				return null;
			}
		}

		public override ChunkDescriptor Pull (byte[] ChunkID)
		{
			foreach (var F in Files) {
				if (F.Value.Hash == ChunkID)
					return F.Value;
			}
			return null;
		}

		public override ParallelPagedEnumerator<ChunkDescriptor> PullAfter (byte[] ChunkID)
		{
			throw new System.NotImplementedException ();
		}

		public override ParallelPagedEnumerator<ChunkDescriptor> Enumerate ()
		{
			Tuple<Peer, Reference<long>> mState = new Tuple<Peer, Reference<long>> (this, new Reference<long> (0, false));
			return new ParallelPagedEnumerator<ChunkDescriptor> (mState, (State) =>
			{
				Tuple<Peer, Reference<long>> lState = (Tuple<Peer, Reference<long>>)State;
				long I;
				ChunkDescriptor ORCI;
				lock (lState.Item2) {
					I = lState.Item2.Value;
					if (I == lState.Item1.Files.Count)
						return null;
					I++;
					ORCI = lState.Item1.Files [I];
					lState.Item2.Value = I;
				}
				//ORCI.GoLive();
				return ORCI;
			}, 8, 2);
		}

		public override ChunkDescriptor GetLastChunk ()
		{
			return Files [Files.Count - 1];
		}

		public override ParallelPagedEnumerator<ChunkDescriptor> GetChunksBackward ()
		{
			return new ParallelPagedEnumerator<ChunkDescriptor> 
				(new Tuple<BSO.Reference<long>, SortedDictionary<long, RemoteChunk>> (new Reference<long> (Files.Count, false), Files),
			  (State) => {
				Tuple<BSO.Reference<long>, SortedDictionary<long, RemoteChunk>> TState = (Tuple<BSO.Reference<long>, SortedDictionary<long, RemoteChunk>>)(State);
				lock (State) {
					long V = TState.Item1.Value;
					if (V == 0)
						return null;
					TState.Item1.Value = V - 1;
					return TState.Item2 [V];
				}
			}, 4, 2);
			//return new ParallelPagedEnumerator<ObjectRepositoryChunkInfo> (
		}

		public override Guid ID {
			get {
				using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("SELECT VALUE FROM ATTRIBUTES WHERE NAME = 'ID'", Base)) {
					return new Guid ((byte[])Comm.ExecuteScalar ());
				}	
			}
		}
		#endregion
	}
}
