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
using System.Collections.Generic;
using System.Data;

namespace BD2.Conv.MSSQL
{
	public static class MainClass
	{
		static int numericConfigurationID = 0;
		static readonly SortedDictionary<string, SqlConnectionConfiguration> namedConfigurations = 
			new SortedDictionary<string, SqlConnectionConfiguration> ();
		static readonly SortedDictionary<int, SqlConnectionConfiguration> numericConfigurations = 
			new SortedDictionary<int, SqlConnectionConfiguration> ();

		static string ReadLine ()
		{
			string command;
			command = Console.ReadLine ();
			while (command.EndsWith ("\\", StringComparison.Ordinal))
				command = command.Substring (0, command.Length - 1) + Console.ReadLine ();
			return command;
		}

		static void Load ()
		{
			const string FetchTable = "SELECT * FROM {0}";
			Console.Write ("Connection name: ");
			SqlConnectionConfiguration sqlcc;
			string name;
			name = ReadLine ();
			sqlcc = namedConfigurations [name];
			SortedSet<STable> tables = sqlcc.GetTableStore ("Original");
			if (tables.Count != 0) {
				Console.WriteLine ("deleting data from provious operations.");
				tables.Clear ();
			}
			using (IDbConnection conn_tables = sqlcc.GetConnection ()) {
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
							Console.WriteLine ("{0}\t, {1}: ", TableId, TableName);
							const bool CaseSensitiveTableMetaData = true;
							STable table = new STable (TableId, TableName, CaseSensitiveTableMetaData);
							using (IDbConnection conn_columns = sqlcc.GetConnection ()) {
								conn_columns.Open ();
								using (IDbCommand comm_columns = conn_columns.CreateCommand ()) {
									const string ListColumnsQuery = "Select * from sys.columns where object_id = @id";
									IDbDataParameter param_id_columns = comm_columns.CreateParameter ();
									param_id_columns.ParameterName = "id";
									param_id_columns.Value = TableId;
									comm_columns.CommandText = ListColumnsQuery;
									comm_columns.Parameters.Add (param_id_columns);
									using (IDataReader reader_columns = comm_columns.ExecuteReader ()) {
										int NameFieldOrdinal_Columns = reader_columns.GetOrdinal ("name");
										int NullableFieldOrdinal_Columns = reader_columns.GetOrdinal ("is_nullable");
										while (reader_columns.Read ()) {
											string ColumnName = reader_columns.GetString (NameFieldOrdinal_Columns);
											bool ColumnNullable = reader_columns.GetBoolean (NullableFieldOrdinal_Columns);
											table.AddColumn (ColumnName, !ColumnNullable);
											Console.Write (ColumnName + "\t");
										}
										Console.WriteLine ();
									}
								}
							}
							tables.Add (table);
						}
					}
				}
				Console.Write (".");
				System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch ();
				sw1.Start ();
				int trc = 0;
				foreach (STable st in tables) {
					Console.Write (st.Name.PadRight (32));
					using (IDbCommand comm_table = conn_tables.CreateCommand ()) {
						string FetchTableQuery = string.Format (FetchTable, st.Name);
						comm_table.CommandText = FetchTableQuery;
						using (IDataReader reader_table = comm_table.ExecuteReader ()) {
							SColumn[] columns = new SColumn[reader_table.FieldCount];
							for (int n = 0; n != reader_table.FieldCount; n++) {
								columns [n] = (SColumn)st.GetColumnByName (reader_table.GetName (n));
							}
							int rc = 0;
							while (reader_table.Read ()) {
								if ((rc & 1023) == 0)
									Console.Write (".");
								SRow row = st.AddRow ();
								for (int n = 0; n != reader_table.FieldCount; n++) {
									row.SetValue (columns [n], reader_table.GetValue (n));
								}
								rc++;
							}
							trc += rc;
							Console.WriteLine ("\t" + rc.ToString () + " rows loaded.");
						}
					}
				}
				sw1.Stop ();
				Console.WriteLine ();
				Console.WriteLine (trc.ToString () + " total rows loaded in " + tables.Count + " tables, openation took " + sw1.ElapsedMilliseconds + "ms.");
			}
		}

		static void Open ()
		{
			string name;
			int z;
			do {
				Console.Write ("name: ");
				name = ReadLine ();
			} while (int.TryParse (name, out z));
			Console.Write ("Server: ");
			string server;
			server = ReadLine ();
			Console.Write ("Protocol: ");
			string protocol;
			protocol = ReadLine ();
			//TODO: Add support for integrated security
			Console.Write ("User Name: ");
			string userName;
			userName = ReadLine ();
			Console.Write ("Password: ");
			string password;
			password = ReadLine ();
			Console.Write ("Catalog: ");
			string catalog;
			catalog = ReadLine ();
			SqlConnectionConfiguration sqlcc = new SqlConnectionConfiguration ();
			sqlcc.ApplicationName = "BD2 SQL Converter";
			sqlcc.ConnectionProtocol = protocol;
			sqlcc.DatabaseName = catalog;
			sqlcc.Password = password;
			sqlcc.Server = server;
			sqlcc.UserName = userName;
			sqlcc.Id = ++numericConfigurationID;
			numericConfigurations.Add (numericConfigurationID, sqlcc);
			namedConfigurations.Add (name, sqlcc);
		}

		static void Close ()
		{
			Console.Write ("Connection name: ");
			SqlConnectionConfiguration sqlcc;
			string name;
			name = ReadLine ();
			sqlcc = namedConfigurations [name];
			namedConfigurations.Remove (name);
			numericConfigurations.Remove (sqlcc.Id);
		}

		static void Test ()
		{
			Console.Write ("Connection name: ");
			SqlConnectionConfiguration sqlcc;
			string name;
			name = ReadLine ();
			sqlcc = namedConfigurations [name];
			using (IDbConnection conn = sqlcc.GetConnection ()) {
				try {
					conn.Open ();
					conn.Close ();
					Console.WriteLine ("passed.");
				} catch (Exception ex) {
					Console.Write ("failed with error, ");
					Console.WriteLine (ex.Message);
					Console.WriteLine ("press R to throw.");
					if (Console.ReadKey ().Key == ConsoleKey.R)
						throw;
				}
			}
		}

		static void Execute ()
		{
			Console.Write ("Connection name: ");
			SqlConnectionConfiguration sqlcc;
			string name;
			name = ReadLine ();
			sqlcc = namedConfigurations [name];
			IDbConnection conn = sqlcc.GetConnection ();
			conn.Open ();
			string Command = "";
			string CommandPart;
			while ((CommandPart = Console.ReadLine ()) != "")
				Command += CommandPart;
			IDbCommand comm = conn.CreateCommand ();
			comm.CommandText = Command;
			try {
				System.Data.IDataReader DR;
				using (DR = comm.ExecuteReader ()) {
					int FC = DR.FieldCount;
					object[] Objects = new object[FC];
					int ResultSetID = 0;
					do {
						Console.WriteLine ("Result Set: {0}", ResultSetID++);
						while (DR.Read ()) {
							DR.GetValues (Objects);
							for (int n = 0; n != FC; n++) {
								object Obj = Objects [n];
								if (DR.IsDBNull (n))
									Console.WriteLine (DR.GetName (n).PadLeft (32) + ": " + "<null>");
								else if (Obj is string)
									Console.WriteLine (DR.GetName (n).PadLeft (32) + ": \"" + Obj.ToString ().Replace ("\\", "\\\\").Replace ("\"", "\\\"").Replace (" ", "\\ ").Replace ("\n", "\\n").Replace ("\r", "\\r").Replace ("\t", "\\t") + "\"");
								else if (Obj is byte[])
									Console.WriteLine (DR.GetName (n).PadLeft (32) + ": 0x" + ((byte[])Obj).ToHexadecimal () + "");
								else
									Console.WriteLine (DR.GetName (n).PadLeft (32) + ": " + Obj.ToString ());
							}
							Console.WriteLine ();
							ConsoleKeyInfo cki = Console.ReadKey ();
							if (cki.Key == ConsoleKey.Q) {
								Console.WriteLine ();
								return;
							}
							Console.SetCursorPosition (0, Console.CursorTop);
						}
					} while (DR.NextResult ());
					Console.WriteLine ();
				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			conn.Close ();
		}

		static void Convert ()
		{
			Console.Write ("Connection name: ");
			SqlConnectionConfiguration sqlcc;
			string name;
			name = ReadLine ();
			sqlcc = namedConfigurations [name];
			SortedSet<STable> originals = sqlcc.GetTableStore ("Original");
			SortedSet<STable> converted = sqlcc.GetTableStore ("Converted");

		}

		public static void Main ()
		{
			bool End = false;
			while (!End) {
				Console.Write ("bd2> ");
				string command;
				command = ReadLine ();
				switch (command) {
				case "open":
					Open ();
					break;
				case "close":
					Close ();
					break;
				case "test":
					Test ();
					break;
				case "load":
					Load ();
					break;
				case "convert":
					Convert ();
					break;
				case "exec":
					Execute ();
					break;
				case "exit":
					End = true;
					break;
				}
			}
		}
	}
}
