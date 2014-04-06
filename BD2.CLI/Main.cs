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
using System.Collections.Generic;
using BD2.Common;
using BD2.Conv.Frontend.Table;

namespace BD2.CLI
{
	class MainClass
	{
		static object lock_query = new object ();

		public static string Query (string message)
		{
			//yes, it's just this simple for now.
			lock (lock_query) {
				Console.Write (message + " ");
				return Console.ReadLine ();
			}
		}

		static SortedSet<string> Modifiers = new SortedSet<string> ();

		public static int ExtractModifiers (string[] parts)
		{
			for (int n = 0; n != parts.Length; n++) {
				if (!Modifiers.Contains (parts [n]))
					return n;
			}
			return -1;
		}

		static void DoConvert (BD2.Daemon.TransparentAgent agent)
		{
			string repoName = Query ("Destination repository");

			BD2.Frontend.Table.Frontend frontend = new BD2.Frontend.Table.Frontend (null);
			BD2.Core.Frontend[] frontends = new BD2.Core.Frontend[] { frontend };
			BD2.Core.Database db = new BD2.Core.Database (new BD2.Chunk.ChunkRepository[] { repositories[repoName] }, frontends, repoName);
			BD2.Core.Snapshot ss = db.CreateSnapshot ("Current");
			BD2.Frontend.Table.FrontendInstance frontendInstance = (BD2.Frontend.Table.FrontendInstance)frontend.CreateInstanse (ss);
			SortedDictionary<Table, List<Column>> tableColumns = new SortedDictionary<Table, List<Column>> ();
			SortedDictionary<Guid, Tuple<Table, List<Column>>> tables = new SortedDictionary<Guid, Tuple<Table, List<Column>>> ();
			SortedDictionary<Guid, Table> tableDataRequests = new SortedDictionary<Guid, Table> ();

			System.Threading.ManualResetEvent MRE = new System.Threading.ManualResetEvent (false);
			int pendingTables = -1;

			agent.RegisterType (typeof(GetRowsRequestMessage), (message) => { });
			agent.RegisterType (typeof(GetRowsResponseMessage), (message) => {
				GetRowsResponseMessage GRRM = (GetRowsResponseMessage)message;
				Table table = tableDataRequests [GRRM.RequestID];
				//BD2.Frontend.Table.Table frontendTable = 
				new BD2.Frontend.Table.Table (frontendInstance, null, table.Name);
				foreach (Column c in tableColumns[table]) {
					//BD2.Frontend.Table.Column frontendColumn = 
					new BD2.Frontend.Table.Column (frontendInstance, null, c.Name, 0, !c.Mandatory, c.Size);
					//TODO:Avoid creating duplicates,create columnsets for each table, Associate columnSets and tables, Add data to tables
				}
			});
			agent.RegisterType (typeof(GetColumnsRequestMessage), (message) => { });
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
					pendingTables --;
					if (pendingTables == 0)
						MRE.Set ();
				}
			});
			agent.RegisterType (typeof(GetTablesRequestMessage), (message) => { });
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
					pendingTables = tables.Count;
				}
			});
			agent.SendMessage (new GetTablesRequestMessage (Guid.NewGuid ()));
			MRE.WaitOne ();
			MRE.Dispose ();


			foreach (var tuple in tables) {
				Table table = tuple.Value.Item1;
				Guid reqID = Guid.NewGuid ();
				tableDataRequests.Add (reqID, table);
				agent.SendMessage (new GetRowsRequestMessage (reqID, table.ID));

			}
		}

		static SortedDictionary<string, BD2.Chunk.ChunkRepository> repositories;

		public static void Main (string[] args)
		{

			SortedDictionary<int, System.Threading.Thread> jobs = new SortedDictionary<int, System.Threading.Thread> ();
			repositories = new SortedDictionary<string, BD2.Chunk.ChunkRepository> ();
			Modifiers.Add ("async");
			//BD2.Core.Database DB = new BD2.Core.Database ();
			string command;
			do {
				command = Query ("Command>");
				OffsetedArray<string> commandparts = command.Split (' ');
				string[] CommandParts = (string[])((string[])commandparts).Clone ();
				commandparts.Offset = ExtractModifiers (CommandParts);
				SortedSet<string> CommandModifiers = new SortedSet<string> (commandparts.GetStrippedPart ());
				switch (CommandParts [0]) {
				case "Run":
					if (CommandParts.Length < 2) {
						Console.Error.WriteLine ("Run requires at least two parameter: [async] Run <Type>");
						continue;
					}
					switch (CommandParts [1]) {
					case "Daemon":
						if (CommandParts.Length < 3) {
							Console.Error.WriteLine ("Run Daemon requires at least one parameter: [async] Run Daemon <Daemon Type>");
							continue;
						}
						switch (CommandParts [2]) {
						case "SQL":
							if (CommandModifiers.Contains ("async")) {
								System.Threading.Thread t_runSourceDaemon = new System.Threading.Thread (RunSourceDaemon);
								jobs.Add (t_runSourceDaemon.ManagedThreadId, t_runSourceDaemon);
								t_runSourceDaemon.Start ();
							} else
								RunSourceDaemon ();
							break;//source
						default:
							Console.Error.WriteLine ("Invalid Daemon Type.");
							break;
						}
						break;
					}
					break;//daemon
				case "Open":
					if (CommandParts.Length < 2) {
						Console.Error.WriteLine ("Open requires at least one parameter: [async] Open <Repository Type>");
						continue;
					}
					switch (CommandParts [1]) {
					case "LevelDB":
						if (CommandParts.Length < 4) {
							Console.Error.WriteLine ("File repository requires at least two parameter: [async] Open File <Repository Nick> <Repository Path>");
							continue;
						}
						if (CommandModifiers.Contains ("async")) {
							System.Threading.Thread t_addrepo = new System.Threading.Thread (() =>
							{
								string nick = CommandParts [2];
								string path = CommandParts [3];
								BD2.Chunk.ChunkRepository LRepo = new BD2.Repo.Leveldb.Repository (path);
								lock (repositories)
									repositories.Add (nick, LRepo);
							});
							t_addrepo.Start ();
							Console.WriteLine ("[{0}]", t_addrepo.ManagedThreadId);
						} else {
							BD2.Chunk.ChunkRepository LRepo = new BD2.Repo.Leveldb.Repository (CommandParts [3]);
							lock (repositories)
								repositories.Add (CommandParts [2], LRepo);
						}
						break;//file
					case "Network":
						break;
					case "Socket":
						break;
					}
					break;
				case "Close":
					break;
				case "Execute":

					break;
				case "Convert":
					if (CommandParts.Length < 2) {
						Console.Error.WriteLine ("Open requires at least one parameter: [async] Open <Repository Type> [Parameters]");
						continue;
					}
					switch (CommandParts [1]) {
					case "Table":
						string daemonIPAddress = Query ("Daemon IP Address");
						string DaemonPort = Query ("Daemon Port");
						System.Net.IPAddress IPA = System.Net.IPAddress.Parse (daemonIPAddress);
						System.Net.IPEndPoint IPEP = new System.Net.IPEndPoint (IPA, int.Parse (DaemonPort));
						System.Net.Sockets.TcpClient TC = new  System.Net.Sockets.TcpClient ();
						TC.Connect (IPEP);
						System.Net.Sockets.NetworkStream NS = TC.GetStream ();
						BD2.Daemon.StreamHandler SH = new BD2.Daemon.StreamHandler (NS);
						BD2.Daemon.ObjectBus OB = new BD2.Daemon.ObjectBus (SH);
						BD2.Daemon.ServiceManager SM = new BD2.Daemon.ServiceManager (OB);
						Console.WriteLine ("Waiting 15 seconds for remote to anounce all it's services...");
						System.Threading.Thread.Sleep (15000);
						Guid serviceType_SQL = Guid.Parse ("57ce8883-1010-41ec-96da-41d36c64d65d");
						var RS = SM.EnumerateRemoteServices ();
						BD2.Daemon.ServiceAnnounceMessage TSA = null;
						foreach (var SA in RS) {
							if (SA.Type == serviceType_SQL) {
								TSA = SA;
							}
						}
						if (TSA == null) {
							Console.WriteLine ("Required services for Table Conversion not found on remote host.");
						}

						BD2.Daemon.TransparentAgent agent = 
							(BD2.Daemon.TransparentAgent)
								SM.RequestService (TSA, (new BD2.Conv.Daemon.MSSQL.ServiceParameters (Query ("Connection String"))).Serialize (),
						                     BD2.Daemon.TransparentAgent.CreateAgent, null);
						DoConvert (agent);
						break;
					}
					break;
				case "await":
					string jobID = commandparts [1];
					int intJobID = int.Parse (jobID);
					jobs [intJobID].Join ();
					break;
				default:
					Console.Error.WriteLine (string.Format ("{0} is not a valid command.", CommandParts [0]));
					break;
				}
			} while(true);
		}

		static void RunSourceDaemon ()
		{
			string listenAddress = Query ("Listen Address");
			string listenPort = Query ("listen Port");
			System.Net.IPAddress IPA = System.Net.IPAddress.Parse (listenAddress);
			System.Net.IPEndPoint IPEP = new System.Net.IPEndPoint (IPA, int.Parse (listenPort));
			System.Net.Sockets.TcpListener TL = new System.Net.Sockets.TcpListener (IPEP);
			TL.Start ();
			System.Net.Sockets.Socket sock = TL.AcceptSocket ();
			System.Net.Sockets.NetworkStream NS = new System.Net.Sockets.NetworkStream (sock);
			BD2.Daemon.StreamHandler SH = new BD2.Daemon.StreamHandler (NS);
			BD2.Daemon.ObjectBus OB = new BD2.Daemon.ObjectBus (SH);
			BD2.Daemon.ServiceManager SM = new BD2.Daemon.ServiceManager (OB);
			Guid serviceType_SQL = Guid.Parse ("57ce8883-1010-41ec-96da-41d36c64d65d");
			Console.WriteLine ("Waiting 5 seconds for remote to get ready...");
			System.Threading.Thread.Sleep (5000);
			SM.AnnounceService (new BD2.Daemon.ServiceAnnounceMessage (Guid.NewGuid (), serviceType_SQL, "Source"), BD2.Conv.Daemon.MSSQL.ServiceAgent.CreateAgent);
		}
	}
}
