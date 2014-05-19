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
using BD2.Conv.Frontend.Table;
using BD2.Daemon;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BD2.Conv.Daemon.MSSQL
{
	public class ServiceAgent : BD2.Daemon.ServiceAgent
	{
		string connectionString;
		PreservingSchemaCollection psc = new PreservingSchemaCollection ();
		SortedDictionary<int, string> tfqns = new SortedDictionary<int, string> ();
		SortedDictionary<int, Table> tables = new SortedDictionary<int, Table> ();
		//SortedDictionary<Guid, Column> columns = new SortedDictionary<Guid, Column> ();
		//SortedDictionary<int, SortedDictionary<string,Column>> tableColumns = new SortedDictionary<int, SortedDictionary<string, Column>> ();
		ServiceAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, ServiceParameters parameters)
			:base(serviceAgentMode, objectBusSession, flush, false)
		{
			if (serviceAgentMode != ServiceAgentMode.Server)
				throw new Exception ("This service agent can only be run in server mode.");
			connectionString = parameters.ConnectionString;
			objectBusSession.RegisterType (typeof(GetTablesRequestMessage), GetTablesRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetColumnsRequestMessage), GetColumnsRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetRowsRequestMessage), GetRowsRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetForeignKeyRelationsRequestMessage), GetForeignKeyRelationsRequestMessageReceived);
			tfqns.Add (48, "System.Byte");
			tfqns.Add (173, "System.Byte[]");
			tfqns.Add (165, "System.Byte[]");
			tfqns.Add (52, "System.Int16");
			tfqns.Add (56, "System.Int32");
			tfqns.Add (127, "System.Int64");
			tfqns.Add (62, "System.Double");
			tfqns.Add (106, "System.Double");
			tfqns.Add (108, "System.Double");
			tfqns.Add (231, "System.String");
			tfqns.Add (239, "System.String");
			tfqns.Add (175, "System.String");
			tfqns.Add (104, "System.Boolean");
			tfqns.Add (36, "System.Guid");
			tfqns.Add (41, "System.DateTime");
			tfqns.Add (42, "System.DateTime");
			tfqns.Add (61, "System.DateTime");

		}

		public static ServiceAgent CreateAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, byte[] parameters)
		{
			return new ServiceAgent (serviceAgentMode, objectBusSession, flush, ServiceParameters.Deserialize (parameters));
		}

		void GetTablesRequestMessageReceived (ObjectBusMessage obj)
		{
			Console.WriteLine ("GetTablesRequestMessageReceived()");
			GetTablesRequestMessage request = (GetTablesRequestMessage)obj;
			GetTablesResponseMessage response;
			try {
				response = new GetTablesResponseMessage (request.ID, (new List<Table> (getTables ())).ToArray (), null);
			} catch (Exception ex) {
				response = new GetTablesResponseMessage (request.ID, new Table[0] { }, ex);
			}
			ObjectBusSession.SendMessage (response);
		}

		void GetColumnsRequestMessageReceived (ObjectBusMessage obj)
		{
			Console.WriteLine ("GetColumnsRequestMessageReceived()");
			GetColumnsRequestMessage request = (GetColumnsRequestMessage)obj;
			GetColumnsResponseMessage response;
			Table table;
			try {
				lock (tables)
					table = (Table)psc.GetTableByID (request.TableID);
				response = new GetColumnsResponseMessage (request.ID, (new List <Column> (getColumns (table.SqlTableID))).ToArray (), null);
			} catch (Exception ex) {
				response = new GetColumnsResponseMessage (request.ID, new Column[0] { }, ex);			
			}
			ObjectBusSession.SendMessage (response);
		}

		class ReadRowsDataContext {
			SqlConnection connection;

			public SqlConnection Connection {
				get {
					return connection;
				}
			}

			SqlCommand command;

			public SqlCommand Command {
				get {
					return command;
				}
			}

			SqlDataReader reader;

			public SqlDataReader Reader {
				get {
					return reader;
				}
			}
			IStream stream;

			public IStream Stream {
				get {
					return stream;
				}
			}
			Table table;

			public Table Table {
				get {
					return table;
				}
			}

			public ReadRowsDataContext(SqlConnection connection, SqlCommand command, SqlDataReader reader, Table table, IStream stream) {
				if (connection == null)
					throw new ArgumentNullException ("connection");
				if (command == null)
					throw new ArgumentNullException ("command");
				if (reader == null)
					throw new ArgumentNullException ("reader");
				if (table == null)
					throw new ArgumentNullException ("table");
				if (stream == null)
					throw new ArgumentNullException ("stream");
				this.connection = connection;
				this.command = command;
				this.reader = reader;
				this.table = table;
				this.stream = stream;
			}
		}
			 
		void GetRowsRequestMessageReceived (ObjectBusMessage obj)
		{
			Console.WriteLine ("GetRowsRequestMessageReceived()");
			GetRowsRequestMessage request = (GetRowsRequestMessage)obj;
			GetRowsResponseMessage response;
			StreamPair SP = new StreamPair ();
			GC.SuppressFinalize (SP);
			Guid StreamID = CreateStream (SP.GetOStream ());
			IStream inStream = SP.GetIStream ();
			Table table;
			lock (tables)
				table = (Table)psc.GetTableByID (request.TableID);
			Console.WriteLine (table.Name);
			//try {
				SqlConnection conn_Rows = new SqlConnection (connectionString);
				conn_Rows.Open ();
				SqlCommand cmdSelect = new SqlCommand (string.Format ("Select * from [{0}]", table.Name), conn_Rows);
				cmdSelect.CommandTimeout *= 30; 
				System.Threading.Thread t_read = new System.Threading.Thread (readRowsData);
				t_read.Start (new ReadRowsDataContext (conn_Rows, cmdSelect, cmdSelect.ExecuteReader (), table, inStream));
				response = new GetRowsResponseMessage (request.ID, StreamID, null);
			//} catch (Exception ex) {
//				DestroyStream (StreamID);
//				response = new GetRowsResponseMessage (request.ID, Guid.Empty, ex);
			//}
			ObjectBusSession.SendMessage (response);
			Console.WriteLine ("Sent GetRowsResponseMessage.");
		}

		void readRowsData(object arg)
		{
			Console.WriteLine ("readRowsData()");
			ReadRowsDataContext context = (ReadRowsDataContext)arg;
			System.IO.BinaryWriter bw = new System.IO.BinaryWriter (context.Stream);
			object[] values = new object[context.Reader.FieldCount];
			//SortedDictionary<string, Column> table = tableColumns [context.Table.SqlTableID];
			string[] rowTFQNs = new string[context.Reader.FieldCount];
			BD2.Conv.Frontend.Table.Column[] cols =  new BD2.Conv.Frontend.Table.Column[context.Reader.FieldCount];
			for (int n = 0; n != context.Reader.FieldCount; n++) {
				cols [n] = psc.GetColumnByName (context.Table, context.Reader.GetName (n));
				rowTFQNs [n] = cols[n].TFQN;
			}
			int rc = 0;
			while (context.Reader.Read ()) {
				rc++;
				context.Reader.GetValues (values);
				for (int n = 0; n != context.Reader.FieldCount; n++) {
					bw.Write ((values [n] == null) || (values [n] == DBNull.Value));
					if ((values [n] != null) && (values [n] != DBNull.Value)) {
						//todo: use tfqn array instead
						switch (rowTFQNs [n]) {
						case "System.Byte":
							bw.Write ((byte)values [n]);
							break;
						case "System.Byte[]":
							bw.Write (((byte[])values [n]).Length);
							bw.Write ((byte[])values [n]);
							break;
						case "System.SByte":
							bw.Write ((sbyte)values [n]);
							break;
						case "System.Int16":
							bw.Write ((short)values [n]);
							break;
						case "System.UInt16":
							bw.Write ((ushort)values [n]);
							break;
						case "System.Int32":
							bw.Write ((int)values [n]);
							break;
						case "System.UInt32":
							bw.Write ((uint)values [n]);
							break;
						case "System.Int64":
							bw.Write ((long)values [n]);
							break;
						case "System.UInt64":
							bw.Write ((ulong)values [n]);
							break;
						case "System.Single":
							bw.Write ((float)values [n]);
							break;
						case "System.Double":
							bw.Write ((double)values [n]);
							break;
						case "System.String":
							bw.Write ((string)values [n]);
							break;
						case "System.Char":
							bw.Write ((char)values [n]);
							break;
						case "System.Boolean":
							bw.Write ((bool)values [n]);
							break;
						}
					}
				}
				if ((rc % 1000) == 0) {
					Console.Write (".");
					for (int n =0; n != values.Length; n++) {
						Console.Write ("{0}:{1}\t", cols[n].Name, values[n]);
					}
					Console.WriteLine ();
				}
			}

			Console.WriteLine (rc);
		}
		void GetForeignKeyRelationsRequestMessageReceived (ObjectBusMessage obj)
		{
			Console.WriteLine ("GetForeignKeyRelationsRequestMessageReceived()");
			GetForeignKeyRelationsRequestMessage request = (GetForeignKeyRelationsRequestMessage)obj;
			GetForeignKeyRelationsResponseMessage response;
			try {
				response = new GetForeignKeyRelationsResponseMessage (request.ID, (new List <ForeignKeyRelation> (getForeignKeyRelations ())).ToArray (), null);
			} catch (Exception ex) {
				response = new GetForeignKeyRelationsResponseMessage (request.ID, new ForeignKeyRelation[0] { }, ex);			
			}
			ObjectBusSession.SendMessage (response);
		}
		SortedSet<ForeignKeyRelation> getForeignKeyRelations(){
			Console.WriteLine ("getForeignKeyRelations()");
			SortedSet<ForeignKeyRelation> foreignKeyRelations = new SortedSet<ForeignKeyRelation> ();
			const string FetchForeignKeyRelation = "SELECT\n" + 
				"    fk.name 'FK Name',\n" + 
				"    tp.object_id 'Parent Table ID',\n" + 
				"    tp.name 'Parent Table Name',\n" + 
				"    cp.name 'Parent Column Name', cp.column_id 'Parent Column ID',\n" + 
				"    tr.object_id 'Child Table ID',\n" + 
				"    tr.name 'Child Table Name',\n" + 
				"    cr.name 'Child Column Name', cr.column_id 'Child Column ID'\n" + 
				"FROM \n" + 
				"    sys.foreign_keys fk\n" + 
				"INNER JOIN \n" + 
				"    sys.tables tp ON fk.parent_object_id = tp.object_id\n" + 
				"INNER JOIN \n" + 
				"    sys.tables tr ON fk.referenced_object_id = tr.object_id\n" + 
				"INNER JOIN \n" + 
				"    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id\n" + 
				"INNER JOIN \n" + 
				"    sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id\n" + 
				"INNER JOIN \n" + 
				"    sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id\n" + 
				"ORDER BY\n" + 
				"    tp.name, cp.column_id";
			using (IDbConnection conn_foreignKeyRelations = new SqlConnection(connectionString)) {
				conn_foreignKeyRelations.Open ();
				using (IDbCommand comm_foreignKeyRelations = conn_foreignKeyRelations.CreateCommand ()) {
					comm_foreignKeyRelations.CommandText = FetchForeignKeyRelation;
					using (IDataReader reader_foreignKeyRelations = comm_foreignKeyRelations.ExecuteReader ()) {
						string lastFKName = null;
						int FKNameFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("FK Name");
						int ParentTableIDFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Parent Table ID");
						//int ParentTableNameFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Parent Table Name");
						int ParentColumnNameFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Parent Column Name");
						//int ParentColumnIDFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Parent Column ID");
						int ChildTableIDFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Child Table ID");
						//int ChildTableNameFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Child Table Name");
						int ChildColumnNameFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Child Column Name");
						//int ChildColumnIDFieldOrdinal_foreignKeyRelations = reader_foreignKeyRelations.GetOrdinal ("Child Column ID");
						List<Guid> parentIDs = null;
						List<Guid> childIDs = null;
						string FKName = null;
						while (reader_foreignKeyRelations.Read ()) {
							FKName = reader_foreignKeyRelations.GetString (FKNameFieldOrdinal_foreignKeyRelations);
							int ParentTableID = reader_foreignKeyRelations.GetInt32 (ParentTableIDFieldOrdinal_foreignKeyRelations);
							//string ParentTableName = reader_foreignKeyRelations.GetString (ParentTableNameFieldOrdinal_foreignKeyRelations);
							string ParentColumnName = reader_foreignKeyRelations.GetString (ParentColumnNameFieldOrdinal_foreignKeyRelations);
							//int ParentColumnID = reader_foreignKeyRelations.GetInt32 (ParentColumnIDFieldOrdinal_foreignKeyRelations);
							int ChildTableID = reader_foreignKeyRelations.GetInt32 (ChildTableIDFieldOrdinal_foreignKeyRelations);
							//string ChildTableName = reader_foreignKeyRelations.GetString (ChildTableNameFieldOrdinal_foreignKeyRelations);
							string ChildColumnName = reader_foreignKeyRelations.GetString (ChildColumnNameFieldOrdinal_foreignKeyRelations);
							//int ChildColumnID = reader_foreignKeyRelations.GetInt32 (ChildColumnIDFieldOrdinal_foreignKeyRelations);
							if (lastFKName != FKName) {
								if (lastFKName != null) {
									foreignKeyRelations.Add (new ForeignKeyRelation (FKName, childIDs.ToArray (), parentIDs.ToArray ()));
								}
								parentIDs = new List<Guid> ();  
								childIDs = new List<Guid> ();
							} 
							//append columns to the current relation;
							parentIDs.Add (psc.GetColumnByName (tables [ParentTableID], ParentColumnName).ID);
							childIDs.Add (psc.GetColumnByName (tables [ChildTableID], ChildColumnName).ID);
							lastFKName = FKName;
						}
						if (FKName != null) {
							foreignKeyRelations.Add (new ForeignKeyRelation (FKName, childIDs.ToArray(), parentIDs.ToArray()));
						}
					}
				}
			}
			return new SortedSet<ForeignKeyRelation> ();
		}

		SortedSet<Table> getTables ()
		{
			Console.WriteLine ("getTables()");
			SortedSet<Table> returnTables = new SortedSet<Table> ();
			const string FetchTable = "SELECT * FROM {0}";
			if (returnTables.Count != 0) {
				Console.WriteLine ("deleting data from provious operations.");
				returnTables.Clear ();
			}
			using (IDbConnection conn_tables = new SqlConnection(connectionString)) {
				conn_tables.Open ();
				using (IDbCommand comm_tables = conn_tables.CreateCommand ()) {
					string ListTablesQuery = string.Format (FetchTable, "sys.tables");
					comm_tables.CommandText = ListTablesQuery;
					using (IDataReader reader_tables = comm_tables.ExecuteReader ()) {
						int NameFieldOrdinal_tables = reader_tables.GetOrdinal ("name");
						int IdFieldOrdinal_tables = reader_tables.GetOrdinal ("object_id");
						while (reader_tables.Read ()) {
							int TableID = reader_tables.GetInt32 (IdFieldOrdinal_tables);
							string TableName = reader_tables.GetString (NameFieldOrdinal_tables);
							Table table = new Table (Guid.NewGuid (), TableName, TableID);
							Console.WriteLine ("table name: {0}",TableName);
							returnTables.Add (table);
							tables.Add (table.SqlTableID, table);
							psc.AddTable (table);
						}
					}
				}
				return returnTables;
			}
		}

		SortedSet<Column> getColumns (int tableID)
		{
			SortedSet<Column> returnColumns =  new SortedSet<Column> ();
			using (IDbConnection conn_columns = new SqlConnection(connectionString)) {
				conn_columns.Open ();
				using (IDbCommand comm_columns = conn_columns.CreateCommand ()) {
					const string ListColumnsQuery = "Select * from sys.columns where object_id = @id";
					IDbDataParameter param_id_columns = comm_columns.CreateParameter ();
					param_id_columns.ParameterName = "id";
					param_id_columns.Value = tableID;
					comm_columns.CommandText = ListColumnsQuery;
					comm_columns.Parameters.Add (param_id_columns);
					using (IDataReader reader_columns = comm_columns.ExecuteReader ()) {
						int OrderFieldOrdinal_Columns = reader_columns.GetOrdinal ("column_id");
						int NameFieldOrdinal_Columns = reader_columns.GetOrdinal ("name");
						int NullableFieldOrdinal_Columns = reader_columns.GetOrdinal ("is_nullable");
						int LengthFieldOrdinal_Columns = reader_columns.GetOrdinal ("max_length");
						int TypeFieldOrdinal_Columns = reader_columns.GetOrdinal ("system_type_id");
						while (reader_columns.Read ()) {
							int ColumnOrder = reader_columns.GetInt32 (OrderFieldOrdinal_Columns);
							string ColumnName = reader_columns.GetString (NameFieldOrdinal_Columns);
							bool ColumnNullable = reader_columns.GetBoolean (NullableFieldOrdinal_Columns);
							short ColumnLength = reader_columns.GetInt16 (LengthFieldOrdinal_Columns);
							byte ColumnType = reader_columns.GetByte (TypeFieldOrdinal_Columns);
							string tfqn = "System.Object";
							if (tfqns.ContainsKey (ColumnType))
								tfqn = tfqns [ColumnType];
							else
								Console.WriteLine ("typeof({0}) is not known.", ColumnName);
							Column column = new Column (Guid.NewGuid (), ColumnName, !ColumnNullable, ColumnLength, tfqn, ColumnOrder);
							returnColumns.Add (column);
							psc.AddColumn (tables [tableID], column);
							Console.Write (ColumnName + "\t");
						}
					}
				}
			}
			return returnColumns;
		}
	}
}