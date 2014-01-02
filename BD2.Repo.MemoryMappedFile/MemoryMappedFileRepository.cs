//
//  MemoryMappedFileRepository.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Collections.Generic;
using BSO;
using BD2.Common;

namespace BD2.Repo.MemoryMappedFile
{
	
	public class MemoryMappedFileRepository : ChunkDataRepository
	{
		SortedSet<ChunkDescriptor> publicChunks;
		Mono.Data.Sqlite.SqliteConnection Base;
		string path;
		string name;
		Guid id;
		public MemoryMappedFileRepository (ChunkDataRepositoryCollection Repo, string Path, string Name)
			: base (Repo)
		{
			if (Repo == null)
				throw new ArgumentNullException ("Repo");
			if (Path == null)
				throw new ArgumentNullException ("Path");
			if (Name == null)
				throw new ArgumentNullException ("Name");
			publicChunks =  new SortedSet<ChunkDescriptor> (new ChunkDescriptor.Comparer_Hash());
			path = Path;
			name = Name;
			string FPath = Path + System.IO.Path.DirectorySeparatorChar + Name;
			bool Exists = System.IO.File.Exists (FPath);
			if (!Exists)
				System.IO.File.Create (FPath);
			Base = new Mono.Data.Sqlite.SqliteConnection ("Data Source=" + FPath);
			if (!Exists) {
				try{
					id = Guid.NewGuid();
				Base.Open();
				using(Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"CREATE TABLE CHUNKS(" +
					"ID BLOB(64) NOT NULL, " +
					"HAS_DEPENDENDANTS BIT NOT NULL, " +
					"Path TEXT(400) NOT NULL)",Base))
					Comm.ExecuteNonQuery();
				using(Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"CREATE TABLE ATTRIBUTES(" +
					"NAME TEXT(128) PRIMARY KEY NOT NULL, " +
					"VALUE BLOB(1024) NOT NULL)",Base))
					Comm.ExecuteNonQuery();
				using(Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"CREATE TABLE DEPENDENCIES(" +
					"ID BLOB(64) NOT NULL, " +
					"DependantIDs BLOB(896) NOT NULL)",Base)) //default sqlite3 page size = 1024 = 64 for ID + 40 for everything sqlite needs + 920 which is a bit bigger than 14 Dependencies(896).
					Comm.ExecuteNonQuery();
				using(Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand (
					"INSERT INTO ATTRIBUTES(" +
					"NAME, " +
					"VALUE) VALUES('ID', @p0)",Base)){
						Comm.Parameters.AddWithValue("p0", ID.ToByteArray());
						Comm.ExecuteNonQuery();
					}
				}
				catch(Exception ex){
					if(Base != null) {
						if(Base.State == System.Data.ConnectionState.Open) {
							Base.Close();
						}
						Base.Dispose();
					}
					if(System.IO.File.Exists (FPath))
						System.IO.File.Delete(FPath);
					throw ex;
				}
			}
			else
				Base.Open();
		}
		~MemoryMappedFileRepository()
		{
			Base.Dispose();
			GC.SuppressFinalize(this);
		}
		public override void Push (ChunkDescriptor Chunk)
		{
			if (Chunk == null)
				throw new ArgumentNullException ("Chunk");
			string NName = Chunk.Hash.ToHexadecimal ();
			string FPath = path +System.IO.Path.DirectorySeparatorChar + NName;
			Stream ChunkBytes = Chunk.GetRawData();
			new File (Chunk, this, FPath, ChunkBytes);
				using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("INSERT INTO Files (ID, Name, Hash, Attributes) VALUES (@p0,@p1,@p2,@p3)", Base)) {
					Comm.Parameters.AddWithValue("p0", Chunk.Hash);
					Comm.Parameters.AddWithValue("p1", NName);
					Comm.Parameters.AddWithValue("p2", null);
					Comm.ExecuteNonQuery ();
				}

		}
		public override ChunkData Pull (ChunkDescriptor Chunk)
		{
			using (Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand ("SELECT PATH FROM CHUNKS WHERE ID = @p0", Base)) {
				Comm.Parameters.AddWithValue("p0", Chunk.Hash);
				Mono.Data.Sqlite.SqliteDataReader DR = Comm.ExecuteReader ();
				if(DR.Read())
				{
					return new File(Chunk, this, DR.GetString(0));
				}
				return null;
			}
		}
		public override IEnumerator<ChunkDescriptor> Enumerate ()
		{
			if (publicChunks == null) {
				populatePublicChunks();
			}
			return publicChunks.GetEnumerator();
		}
		void populatePublicChunks(){

			publicChunks =  new SortedSet<ChunkDescriptor> ();
		}
		public override Guid ID {
			get {
				if(id == Guid.Empty)
				using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("SELECT VALUE FROM ATTRIBUTES WHERE NAME = 'ID'", Base)) {
					id = new Guid((byte[])Comm.ExecuteScalar());
				}
				return id;
			}
		}
	 }
}
