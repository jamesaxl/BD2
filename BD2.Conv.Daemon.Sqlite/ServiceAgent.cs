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
using Mono.Data.Sqlite;

namespace BD2.Conv.Daemon.Sqlite
{
	public class ServiceAgent : BD2.Daemon.ServiceAgent
	{
		SqliteConnection conn;
		SortedDictionary<Guid, Table> tables = new SortedDictionary<Guid, Table> ();
		//SortedDictionary<Guid, Column> columns = new SortedDictionary<Guid, Column> ();
		ServiceAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, ServiceParameters parameters)
			:base(serviceAgentMode, objectBusSession, flush, false)
		{
			if (serviceAgentMode != ServiceAgentMode.Server)
				throw new Exception ("This service agent can only be run in server mode.");
			conn = new SqliteConnection (parameters.ConnectionString);
			objectBusSession.RegisterType (typeof(GetTablesRequestMessage), GetTablesRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetColumnsRequestMessage), GetColumnsRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetRowsRequestMessage), GetRowsRequestMessageReceived);
			objectBusSession.RegisterType (typeof(GetForeignKeyRelationsRequestMessage), GetForeignKeyRelationsRequestMessageReceived);
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
					table = tables [request.TableID];
				response = new GetColumnsResponseMessage (request.ID, (new List <Column> (getColumns (table.Name))).ToArray (), null);
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
				ObjectBusSession.SendMessage (new GetRowsResponseMessage (request.ID, Guid.Empty, new KeyNotFoundException ()));
			}
		}

		void GetForeignKeyRelationsRequestMessageReceived (ObjectBusMessage obj)
		{
			Console.WriteLine ("GetForeignKeyRelationsRequestMessageReceived()");
			//GetForeignKeyRelationsRequestMessage GFKRRM = (GetForeignKeyRelationsRequestMessage)obj;
			//SortedSet<ForeignKeyRelation> FKRs = new SortedSet<ForeignKeyRelation> ();


		}
		private SortedSet<ForeignKeyRelation> getForeignKeyRelations(){
			throw new NotImplementedException ();
		}

		private SortedSet<Table> getTables ()
		{

			using (SqliteCommand command = new SqliteCommand ("select name from sqlite_master where type='table'", conn)) {
				throw new NotImplementedException ();
			
			}

		}

		private SortedSet<Column> getColumns (string tableName)
		{
			throw new NotImplementedException ();
		}

	}
}