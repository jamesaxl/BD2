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
		static Queue<string> initial = new Queue<string> ();

		public static string Query (string message)
		{
			//yes, it's just this simple for now.
			lock (lock_query) {
				if (initial.Count != 0)
					return initial.Dequeue ();
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

		static SortedDictionary<string, BD2.Chunk.ChunkRepository> repositories;

		static void OpenNetworkRepository (string[] commandParts)
		{
			string nick = commandParts [2];
			string ip = commandParts [3];
			string port = commandParts [4];
			System.Net.Sockets.TcpClient TC = new System.Net.Sockets.TcpClient ();
			TC.Connect (new System.Net.IPEndPoint (System.Net.IPAddress.Parse (ip), int.Parse (port)));
			BD2.Chunk.ChunkRepository LRepo = new BD2.Repo.Net.Repository (TC.GetStream ());
			lock (repositories)
				repositories.Add (nick, LRepo);
		}

		static void OpenLevelDBRepository (string[] commandParts)
		{
			string nick = commandParts [2];
			string path = commandParts [3];
			BD2.Chunk.ChunkRepository LRepo = new BD2.Repo.Leveldb.Repository (path);
			lock (repositories)
				repositories.Add (nick, LRepo);
		}

		public static void Main (string[] args)
		{
			if (args.Length == 1)
				foreach (string str in System.IO.File.ReadAllLines (args[0]))
					initial.Enqueue (str);
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
							Console.Error.WriteLine ("LevelDB repository requires at least two parameter: [async] Open File <Repository Nick> <Repository> <Path>");
							continue;
						}
						if (CommandModifiers.Contains ("async")) {
							System.Threading.Thread t_addrepo = new System.Threading.Thread (() =>
							{
								OpenLevelDBRepository (CommandParts);
							});
							t_addrepo.Start ();
							Console.WriteLine ("[{0}]", t_addrepo.ManagedThreadId);
						} else {
							OpenLevelDBRepository (CommandParts);
						}
						break;//file
					case "Network":
						if (CommandParts.Length < 5) {
							Console.Error.WriteLine ("Network repository requires at least three parameter: [async] Open File <Repository Nick> <Repository> <IPAddress> <Port>");
							continue;
						}
						if (CommandModifiers.Contains ("async")) {
							System.Threading.Thread t_addrepo = new System.Threading.Thread (() =>
							{
								OpenNetworkRepository (CommandParts);
							});
							t_addrepo.Start ();
							Console.WriteLine ("[{0}]", t_addrepo.ManagedThreadId);
						} else {
							OpenNetworkRepository (CommandParts);
						}

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
						Console.WriteLine ("Waiting 5 seconds for remote to anounce all it's services...");
						System.Threading.Thread.Sleep (5000);
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
						Client CL = new Client (agent, repositories [Query ("Repository Name")], Query ("Database Name"));
						CL.Convert ();
						break;
					}
					break;
				case "await":
					string jobID = commandparts [1];
					int intJobID = int.Parse (jobID);
					jobs [intJobID].Join ();
					break;
				case "Exit":
					return;
				default:
					Console.Error.WriteLine (string.Format ("{0} is not a valid command.", CommandParts [0]));
					break;
				}
			} while(true);
		}

		static void SourceDaemonListen (object para)
		{
			System.Net.Sockets.TcpListener TL = (System.Net.Sockets.TcpListener)para;
			System.Net.Sockets.Socket sock;
			while ((sock = TL.AcceptSocket ()) != null) {
				System.Net.Sockets.NetworkStream NS = new System.Net.Sockets.NetworkStream (sock);
				BD2.Daemon.StreamHandler SH = new BD2.Daemon.StreamHandler (NS);
				BD2.Daemon.ObjectBus OB = new BD2.Daemon.ObjectBus (SH);
				BD2.Daemon.ServiceManager SM = new BD2.Daemon.ServiceManager (OB);
				Guid serviceType_SQL = Guid.Parse ("57ce8883-1010-41ec-96da-41d36c64d65d");
				Console.WriteLine ("Waiting 1 second for remote to get ready...");
				System.Threading.Thread.Sleep (1000);
				SM.AnnounceService (new BD2.Daemon.ServiceAnnounceMessage (Guid.NewGuid (), serviceType_SQL, "Source"), BD2.Conv.Daemon.MSSQL.ServiceAgent.CreateAgent);
			}
		}

		static void RunSourceDaemon ()
		{
			string listenAddress = Query ("Listen Address");
			string listenPort = Query ("listen Port");
			System.Net.IPAddress IPA = System.Net.IPAddress.Parse (listenAddress);
			System.Net.IPEndPoint IPEP = new System.Net.IPEndPoint (IPA, int.Parse (listenPort));
			System.Net.Sockets.TcpListener TL = new System.Net.Sockets.TcpListener (IPEP);
			TL.Start ();
			System.Threading.Thread listenThread = new System.Threading.Thread (SourceDaemonListen);
			listenThread.Start (TL);
		}
	}
}
