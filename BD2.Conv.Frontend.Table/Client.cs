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
		SortedDictionary<Guid, Table> tableDataRequests;
		BD2.Core.Frontend frontend;
		BD2.Core.Frontend[] frontends;
		BD2.Core.Database db;
		BD2.Core.Snapshot ss;
		BD2.Frontend.Table.FrontendInstance frontendInstance;
		System.Collections.Concurrent.ConcurrentDictionary<Table, System.Collections.Concurrent.BlockingCollection<Column>> tableColumns;
		SortedDictionary<Guid, Tuple<Table, List<Column>>> tables;
		System.Threading.AutoResetEvent AREGetColumns = new System.Threading.AutoResetEvent (false);
		System.Threading.AutoResetEvent AREGetRows = new System.Threading.AutoResetEvent (false);
		Dictionary<Type, long> typeIDs;
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
			typeIDs = new Dictionary<Type, long> ();
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
			tableDataRequests = new SortedDictionary<Guid, Table> ();
			frontend = new BD2.Frontend.Table.Frontend (new BD2.Frontend.Table.GenericValueDeserializer ());
			frontends = new BD2.Core.Frontend[] { frontend };
			db = new BD2.Core.Database (new BD2.Chunk.ChunkRepository[] { repo }, frontends, databaseName);
			ss = db.GetSnapshot ("Primary");
			frontendInstance = (BD2.Frontend.Table.FrontendInstance)frontend.CreateInstanse (ss);
			tableColumns = new System.Collections.Concurrent.ConcurrentDictionary<Table, System.Collections.Concurrent.BlockingCollection<Column>> ();
			tables = new SortedDictionary<Guid, Tuple<Table, List<Column>>> ();
		}

		void GetRowsResponseMessageHandler (GetRowsResponseMessage message)
		{
			Console.WriteLine ("GetRowsResponseMessageReceived()");
			GetRowsResponseMessage GRRM = (GetRowsResponseMessage)message;
			Table table = tableDataRequests [GRRM.RequestID];
			BD2.Frontend.Table.Table frontendTable = new BD2.Frontend.Table.Table (frontendInstance, null, table.Name);
			SortedSet<BD2.Frontend.Table.Model.Column> fcs = new SortedSet<BD2.Frontend.Table.Model.Column> ();
			Console.WriteLine ("Table: {0}", table.Name);
			Console.WriteLine ("Enumerating columns...");
			if (!tableColumns.ContainsKey (table)) {
				Console.WriteLine ("tableColumns doesn't have the key");
			}
			Console.WriteLine ("Column Count: {0}", tableColumns [table].Count);

			foreach (Column c in tableColumns[table]) {
				BD2.Frontend.Table.Model.Column frontendColumn = frontendInstance.GetColumn (c.Name, System.Type.GetType (c.TFQN), !c.Mandatory, c.Size);
				int cc = fcs.Count;
				fcs.Add (frontendColumn);
				if (fcs.Count == cc)
					throw new Exception ("FATAL column ID collision detected");
				//TODO:Avoid creating duplicates,create columnsets for each table, Associate columnSets and tables, Add data to tables
			}
			frontendInstance.Flush ();
			int rc = 0;
			BD2.Frontend.Table.Column[] cols = new BD2.Frontend.Table.Column[fcs.Count];
			fcs.CopyTo (cols);
			BD2.Frontend.Table.Model.ColumnSet columnSet = frontendInstance.GetColumnSet (cols);
			frontendInstance.Flush ();
			foreach (BD2.Conv.Frontend.Table.Row r in message.Rows) {
				frontendInstance.CreateRow (frontendTable, columnSet, r.Fields);
				rc++;
			}
			Console.WriteLine (rc);
			frontendInstance.Flush ();
			AREGetRows.Set ();
		}

		void GetColumnsResponseMessageHandler (GetColumnsResponseMessage message)
		{
			GetColumnsResponseMessage GCRM = (GetColumnsResponseMessage)message;
			Console.WriteLine ("GetColumnsResponseMessage received");
			if (GCRM.Exception != null) {
				Console.WriteLine ("GCRM.Exception: ");
				Console.WriteLine (GCRM.Exception);
			}
			Table table;
			Guid reqID = Guid.NewGuid ();
			Tuple<Table, List<Column>> tuple;
			lock (tables) {
				tuple = tables [GCRM.RequestID];
			}
			Console.Write (tuple.Item1.Name);
			foreach (Column c in GCRM.Columns) {
				tuple.Item2.Add (c);
				tableColumns [tuple.Item1].Add (c);
				Console.WriteLine ("Name:{0}\t, Size:{1}\t, Mandatory:{2}\t, TFQN:{3}", c.Name, c.Size, c.Mandatory, c.TFQN);
			}
			table = tuple.Item1;
			lock (tableDataRequests) {
				tableDataRequests.Add (reqID, table);
			}
			BD2.Conv.Frontend.Table.Row.AddColumnSet (new BD2.Conv.Frontend.Table.ColumnSet (tableColumns [table].ToArray ()));

			agent.SendMessage (new GetRowsRequestMessage (reqID, table.ID));
			AREGetRows.WaitOne ();
			AREGetColumns.Set ();
		}

		void GetTablesResponseMessageHandler (GetTablesResponseMessage message)
		{
			GetTablesResponseMessage GTRM = (GetTablesResponseMessage)message;
			Console.WriteLine ("GetTablesResponseMessage received");
			if (GTRM.Exception != null) {
				Console.WriteLine ("GTRM.Exception: ");
				Console.WriteLine (GTRM.Exception);
			}
			foreach (Table t in GTRM.Tables) {
				Guid reqID = Guid.NewGuid ();
				lock (tables) {
					tables.Add (reqID, new Tuple<Table, List<Column>> (t, new List<Column> ()));
				}
				tableColumns.GetOrAdd (t, (tref) => new System.Collections.Concurrent.BlockingCollection<Column> ());
				Console.WriteLine (t.Name);
				agent.SendMessage (new GetColumnsRequestMessage (reqID, t.ID));
				AREGetColumns.WaitOne ();
			}
		}

		public void Convert ()
		{
			agent.RegisterType (typeof(GetRowsResponseMessage), (message) => {
				GetRowsResponseMessageHandler ((GetRowsResponseMessage)message);
			});
			agent.RegisterType (typeof(GetColumnsResponseMessage), (message) => {
				GetColumnsResponseMessageHandler ((GetColumnsResponseMessage)message);
			});
			agent.RegisterType (typeof(GetTablesResponseMessage), (message) => {
				GetTablesResponseMessageHandler ((GetTablesResponseMessage)message);
			});
			agent.SendMessage (new GetTablesRequestMessage (Guid.NewGuid ()));
		}
	}
}

