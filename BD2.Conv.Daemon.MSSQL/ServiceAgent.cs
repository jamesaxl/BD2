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
		readonly string[] tfqns = new string[2] { "System.Int32", "System.Byte" };
		string connectionString;
		SortedDictionary<Guid, Table> tables;
		SortedDictionary<Guid, Column> columns;

		ServiceAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, ServiceParameters parameters)
			:base(serviceAgentMode, objectBusSession, flush)
		{
			if (serviceAgentMode != ServiceAgentMode.Server)
				throw new Exception ("This service agent can only be run in server mode.");
			connectionString = parameters.ConnectionString;
			objectBusSession.RegisterType (typeof(GetTablesRequestMessage), GetTablesRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetColumnsRequestMessage), GetColumnsRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetRowsRequestMessage), GetRowsRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetForeignKeyRelationsRequestMessage), GetForeignKeyRelationsRequestMessageReceived);

		}

		public static ServiceAgent CreateAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, byte[] parameters)
		{
			return new ServiceAgent (serviceAgentMode, objectBusSession, flush, ServiceParameters.Deserialize (parameters));
		}
		#region implemented abstract members of ServiceAgent
		protected override void Run ()
		{
			throw new NotImplementedException ();
		}

		public override void DestroyRequestReceived ()
		{
			throw new NotImplementedException ();
		}

		public override void SessionDisconnected ()
		{
			throw new NotImplementedException ();
		}
		#endregion
		void GetTablesRequestMessageReceived (BD2.Daemon.ObjectBusMessage obj)
		{
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
			GetColumnsRequestMessage request = (GetColumnsRequestMessage)obj;
			GetColumnsResponseMessage response;
			Table table;
			SortedSet<Column> columns;
			try {
				lock (tables)
					table = tables [request.TableID];
				columns = getColumns (table.SqlTableID);
				response = new GetColumnsResponseMessage (request.ID, (new List <Column> (columns)).ToArray (), null);
			} catch (Exception ex) {
				response = new GetColumnsResponseMessage (request.ID, new Column[0] { }, ex);			
			}
			ObjectBusSession.SendMessage (response);
		}

		void GetRowsRequestMessageReceived (ObjectBusMessage obj)
		{
			GetRowsRequestMessage request = (GetRowsRequestMessage)obj;
			Table t;

			if (tables.TryGetValue (request.TableID, out t)) {
				
			} else {
			
			}
		}

		void GetForeignKeyRelationsRequestMessageReceived (ObjectBusMessage obj)
		{
			throw new NotImplementedException ();
		}

		private SortedSet<Table> getTables ()
		{
			SortedSet<Table> tables = new SortedSet<Table> ();
			const string FetchTable = "SELECT * FROM {0}";
			if (tables.Count != 0) {
				Console.WriteLine ("deleting data from provious operations.");
				tables.Clear ();
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
							int TableId = reader_tables.GetInt32 (IdFieldOrdinal_tables);
							string TableName = reader_tables.GetString (NameFieldOrdinal_tables);
							Table table = new Table (Guid.NewGuid (), TableName, TableId);
							tables.Add (table);
							lock(this.tables)
								this.tables.Add (table.ID, table);
						}
					}
				}
				return tables;
			}
		}

		private SortedSet<Column> getColumns (int tableID)
		{
			SortedSet<Column> columns =  new SortedSet<Column> ();
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
						int NameFieldOrdinal_Columns = reader_columns.GetOrdinal ("name");
						int NullableFieldOrdinal_Columns = reader_columns.GetOrdinal ("is_nullable");
						while (reader_columns.Read ()) {
							string ColumnName = reader_columns.GetString (NameFieldOrdinal_Columns);
							bool ColumnNullable = reader_columns.GetBoolean (NullableFieldOrdinal_Columns);
							columns.Add (new Column (Guid.NewGuid (), ColumnName, !ColumnNullable, 0, null));
							Console.Write (ColumnName + "\t");
						}
						Console.WriteLine ();
					}
				}
			}
			return columns;
		}
	}
}