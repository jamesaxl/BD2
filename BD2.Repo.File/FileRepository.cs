using System;
using System.IO;
using System.Collections.Generic;
using BSO;
using BD2.Common;

namespace BD2.Repo.File
{
	public class Repository : ChunkRepository
	{
		SortedSet<byte[]> publicChunks;
		Mono.Data.Sqlite.SqliteConnection Base;
		string path;
		string name;
		Guid id;

		public Repository (ChunkRepositoryCollection Repo, string Path, string Name)
			: base (Repo)
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
				} catch (Exception ex) {
					if (Base != null) {
						if (Base.State == System.Data.ConnectionState.Open) {
							Base.Close ();
						}
						Base.Dispose ();
					}
					if (System.IO.File.Exists (FPath))
						System.IO.File.Delete (FPath);
					throw ex;
				}
			} else
				Base.Open ();
		}

		~Repository ()
		{
			Base.Close ();
			Base.Dispose ();
		}

		public override void Push (byte[] ChunkDescriptor, byte[] Data)
		{
			if (ChunkDescriptor == null)
				throw new ArgumentNullException ("ChunkDescriptor");
			string NName = ChunkDescriptor.ToHexadecimal ();
			string FPath = path + System.IO.Path.DirectorySeparatorChar + NName;
			new File (ChunkDescriptor, this, FPath, Data);
			using (Mono.Data.Sqlite.SqliteCommand Comm = new Mono.Data.Sqlite.SqliteCommand ("INSERT INTO Files (ID, Name, Hash, Attributes) VALUES (@p0,@p1,@p2,@p3)", Base)) {
				Comm.Parameters.AddWithValue ("p0", ChunkDescriptor);
				Comm.Parameters.AddWithValue ("p1", NName);
				Comm.Parameters.AddWithValue ("p2", null);
				Comm.ExecuteNonQuery ();
			}

		}

		public override byte[] Pull (byte[] ChunkDescriptor)
		{
			using (Mono.Data.Sqlite.SqliteCommand Comm  = new Mono.Data.Sqlite.SqliteCommand ("SELECT PATH FROM CHUNKS WHERE ID = @p0", Base)) {
				Comm.Parameters.AddWithValue ("p0", ChunkDescriptor);
				Mono.Data.Sqlite.SqliteDataReader DR = Comm.ExecuteReader ();
				if (DR.Read ()) {
					return new File (ChunkDescriptor, this, DR.GetString (0));
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
	}
}
