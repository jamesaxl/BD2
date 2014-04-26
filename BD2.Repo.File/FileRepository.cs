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

namespace BD2.Repo.File
{
	public class Repository : ChunkRepository
	{
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
		#endregion
		SortedSet<byte[]> publicChunks;
		Mono.Data.Sqlite.SqliteConnection Base;
		string path;
		string name;

		public string Name {
			get {
				return name;
			}
		}

		Guid id;

		public Repository (IEnumerable<ChunkRepositoryCollection> Repos, string Path, string Name)
			: base (Repos)
		{
			if (Name == null)
				throw new ArgumentNullException ("Name");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			publicChunks = new SortedSet<byte[]> ();
			path = Path;
			name = Name;
			string FPath = Path + System.IO.Path.DirectorySeparatorChar + Name;
			bool Exists = System.IO.File.Exists (FPath);
			if (!Exists)
				System.IO.File.Create (FPath);
			Base = new Mono.Data.Sqlite.SqliteConnection ("Data Source=" + FPath);
			if (!Exists) {
				try {
					id = Guid.NewGuid ();
					Base.Open ();
					using (Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"CREATE TABLE CHUNKS(" +
					"ID BLOB(64) NOT NULL, " +
					"Path TEXT(400) NOT NULL)",Base))
						Comm.ExecuteNonQuery ();
					using (Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"CREATE TABLE ATTRIBUTES(" +
					"NAME TEXT(128) PRIMARY KEY NOT NULL, " +
					"VALUE BLOB(1024) NOT NULL)",Base))
						Comm.ExecuteNonQuery ();
					using (Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"INSERT INTO ATTRIBUTES(" +
					"NAME, " +
					"VALUE) VALUES('ID', @p0)",Base)) {
						Comm.Parameters.AddWithValue ("p0", ID.ToByteArray ());
						Comm.ExecuteNonQuery ();
					}
				} catch (Exception) {
					if (Base != null) {
						if (Base.State == System.Data.ConnectionState.Open) {
							Base.Close ();
						}
						Base.Dispose ();
					}
					throw;
				}
			} else
				Base.Open ();
		}

		~Repository ()
		{
			Base.Close ();
			Base.Dispose ();
		}

		public  void Push (byte[] ChunkDescriptor, byte[] Data)
		{
			if (ChunkDescriptor == null)
				throw new ArgumentNullException ("ChunkDescriptor");
			//TODO:FIX
			string NName = "TODO:FIX";//ChunkDescriptor.ToHexadecimal ();
			string FPath = path + System.IO.Path.DirectorySeparatorChar + NName;
			System.IO.File.WriteAllBytes (FPath, Data);
			using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("INSERT INTO Files (ID, Name, Hash, Attributes) VALUES (@p0,@p1,@p2,@p3)", Base)) {
				Comm.Parameters.AddWithValue ("p0", ChunkDescriptor);
				Comm.Parameters.AddWithValue ("p1", NName);
				Comm.Parameters.AddWithValue ("p2", null);
				Comm.ExecuteNonQuery ();
			}

		}

		public  byte[] Pull (byte[] ChunkDescriptor)
		{
			using (Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand ("SELECT PATH FROM CHUNKS WHERE ID = @p0", Base)) {
				Comm.Parameters.AddWithValue ("p0", ChunkDescriptor);
				Mono.Data.Sqlite.SqliteDataReader DR = Comm.ExecuteReader ();
				string NName = "TODO:FIX";//ChunkDescriptor.ToHexadecimal ();
				string FPath = path + System.IO.Path.DirectorySeparatorChar + NName;
				if (DR.Read ()) {
					return System.IO.File.ReadAllBytes (FPath);
				}
				return null;
			}
		}

		public override IEnumerable<byte[]> Enumerate ()
		{
			if (publicChunks == null) {
				populatePublicChunks ();
			}
			return publicChunks;
		}

		void populatePublicChunks ()
		{

			publicChunks = new SortedSet<byte[]> ();
		}

		public override Guid ID {
			get {
				if (id == Guid.Empty)
					using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("SELECT VALUE FROM ATTRIBUTES WHERE NAME = 'ID'", Base)) {
						id = new Guid ((byte[])Comm.ExecuteScalar ());
					}
				return id;
			}
		}
		#region implemented abstract members of ChunkRepository
		public override void PushIndex (byte[] index, byte[] value)
		{
			throw new NotImplementedException ();
		}

		public override byte[] PullIndex (byte[] index)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
