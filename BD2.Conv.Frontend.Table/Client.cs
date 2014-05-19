/*
  * Copyright (c) 2014 Behrooz Amoozad
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
using System.Collections.Generic;

namespace BD2.Conv.Frontend.Table
{
	public class Client
	{
		Dictionary<Type, long> typeIDs = new Dictionary<Type, long> ();
		BD2.Daemon.TransparentAgent agent;
		BD2.Chunk.ChunkRepository repo;

		public BD2.Chunk.ChunkRepository Repo {
			get {
				return repo;
			}
		}

		string databaseName;

		public string DatabaseName {
			get {
				return databaseName;
			}
		}

		public Client (BD2.Daemon.TransparentAgent agent, BD2.Chunk.ChunkRepository repo, string databaseName)
		{
			if (agent == null)
				throw new ArgumentNullException ("agent");
			if (repo == null)
				throw new ArgumentNullException ("repo");
			if (databaseName == null)
				throw new ArgumentNullException ("databaseName");
			this.agent = agent;
			this.repo = repo;
			this.databaseName = databaseName;
			typeIDs.Add (typeof(bool), 1);
			typeIDs.Add (typeof(char), 2);
			typeIDs.Add (typeof(byte), 3);
			typeIDs.Add (typeof(byte[]), 4);
			typeIDs.Add (typeof(short), 5);
			typeIDs.Add (typeof(int), 6);
			typeIDs.Add (typeof(long), 7);
			typeIDs.Add (typeof(float), 8);
			typeIDs.Add (typeof(double), 9);
			typeIDs.Add (typeof(Guid), 10);
			typeIDs.Add (typeof(String), 11);
			typeIDs.Add (typeof(DateTime), 12);

		}

		public void Convert ()
		{
			SortedDictionary<Guid,Table> tableDataRequests = new SortedDictionary<Guid, Table> ();
			BD2.Frontend.Table.Frontend frontend = new BD2.Frontend.Table.Frontend (new BD2.Frontend.Table.GenericValueDeserializer ());
			BD2.Core.Frontend[] frontends = new BD2.Core.Frontend[] { frontend };
			BD2.Core.Database db = new BD2.Core.Database (new BD2.Chunk.ChunkRepository[] { repo }, frontends, databaseName);
			BD2.Core.Snapshot ss = db.CreateSnapshot ("Current");
			BD2.Frontend.Table.FrontendInstance frontendInstance = (BD2.Frontend.Table.FrontendInstance)frontend.CreateInstanse (ss);
			SortedDictionary<Table, List<Column>> tableColumns = new SortedDictionary<Table, List<Column>> ();
			SortedDictionary<Guid, Tuple<Table, List<Column>>> tables = new SortedDictionary<Guid, Tuple<Table, List<Column>>> ();


			agent.RegisterType (typeof(GetRowsResponseMessage), (message) => {
				Console.WriteLine ("GetRowsResponseMessageReceived()");
				GetRowsResponseMessage GRRM = (GetRowsResponseMessage)message;
				Table table = tableDataRequests [GRRM.RequestID];
				BD2.Frontend.Table.Table frontendTable = new BD2.Frontend.Table.Table (frontendInstance, null, table.Name);
				SortedSet<BD2.Frontend.Table.Model.Column> fcs = new SortedSet<BD2.Frontend.Table.Model.Column> ();
				Console.WriteLine ("Enumerating columns...");
				if (!tableColumns.ContainsKey (table)) {
					Console.WriteLine ("tableColumns doesn't have the key");
				}
				Console.Write ("Column Count:");
				Console.WriteLine (tableColumns [table].Count);
				foreach (Column c in tableColumns[table]) {
					BD2.Frontend.Table.Model.Column frontendColumn = frontendInstance.GetColumn (c.Name, 0, !c.Mandatory, c.Size);
					fcs.Add (frontendColumn);
					Console.Write (".");
					//TODO:Avoid creating duplicates,create columnsets for each table, Associate columnSets and tables, Add data to tables
				}

				Console.WriteLine ("Openning stream...");
				BD2.Daemon.TransparentStream stream = agent.OpenStream (GRRM.ResponseStreamID);
				Console.WriteLine ("Stream created...");
				System.IO.BinaryReader br = new System.IO.BinaryReader (stream);
				Console.WriteLine ("Reading stream...");
				while (true) {
					if (stream.Done) {
						break;
					}
					object[] objects = new object[fcs.Count];
					BD2.Frontend.Table.Column[] cols = new BD2.Frontend.Table.Column[fcs.Count];
					fcs.CopyTo (cols);
					int n = 0;
					Console.WriteLine ("Enumerating columns");
					foreach (Column c in tableColumns[table]) {
						switch (c.TFQN) {
						case "System.Byte[]":
							objects [n] = br.ReadBytes (br.ReadInt32 ());
							break;
						case "System.Byte":
							objects [n] = br.ReadByte ();
							break;
						case "System.SByte":
							objects [n] = br.ReadSByte ();
							break;
						case "System.Int16":
							objects [n] = br.ReadInt16 ();
							break;
						case "System.UInt16":
							objects [n] = br.ReadUInt16 ();
							break;
						case "System.Int32":
							objects [n] = br.ReadInt32 ();
							break;
						case "System.UInt32":
							objects [n] = br.ReadUInt32 ();
							break;
						case "System.Int64":
							objects [n] = br.ReadInt64 ();
							break;
						case "System.UInt64":
							objects [n] = br.ReadUInt64 ();
							break;
						case "System.Single":
							objects [n] = br.ReadSingle ();
							break;
						case "System.Double":
							objects [n] = br.ReadDouble ();
							break;
						case "System.String":
							objects [n] = br.ReadString ();
							break;
						case "System.Char":
							objects [n] = br.ReadChar ();
							break;
						case "System.Boolean":
							objects [n] = br.ReadBoolean ();
							break;
						}

					}
					Console.Write ("Fetched a row");
					frontendInstance.CreateRow (frontendTable, frontendInstance.GetColumnSet (cols), objects);

					frontendInstance.Flush ();
				}
			});
			agent.RegisterType (typeof(GetColumnsResponseMessage), (message) => {
				GetColumnsResponseMessage GCRM = (GetColumnsResponseMessage)message;
				Console.WriteLine ("GetColumnsResponseMessage received");
				if (GCRM.Exception != null) {
					Console.WriteLine ("GCRM.Exception: ");
					Console.WriteLine (GCRM.Exception);
				}
				lock (tables) {
					Tuple<Table, List<Column>> tuple = tables [GCRM.RequestID];
					Console.Write (tuple.Item1.Name);
					foreach (Column c in GCRM.Columns) {
						tuple.Item2.Add (c);
						tableColumns [tuple.Item1].Add (c);
						Console.WriteLine ("Name:{0}\t, Size:{1}\t, Mandatory:{2}\t, TFQN:{3}", c.Name, c.Size, c.Mandatory, c.TFQN);
					}
					Table table = tuple.Item1;
					Guid reqID = Guid.NewGuid ();
					tableDataRequests.Add (reqID, table);
					agent.SendMessage (new GetRowsRequestMessage (reqID, table.ID));

				}
			});
			agent.RegisterType (typeof(GetTablesResponseMessage), (message) => {
				GetTablesResponseMessage GTRM = (GetTablesResponseMessage)message;
				Console.WriteLine ("GetTablesResponseMessage received");
				if (GTRM.Exception != null) {
					Console.WriteLine ("GTRM.Exception: ");
					Console.WriteLine (GTRM.Exception);
				}
				lock (tables) {
					foreach (Table t in  GTRM.Tables) {
						Guid reqID = Guid.NewGuid ();
						tables.Add (reqID, new Tuple<Table, List<Column>> (t, new List<Column> ()));
						tableColumns.Add (t, new  List<Column> ());
						agent.SendMessage (new GetColumnsRequestMessage (reqID, t.ID));
						Console.WriteLine (t.Name);
					}
				}
			});
			agent.SendMessage (new GetTablesRequestMessage (Guid.NewGuid ()));

		}
	}
}

