using System;
using BD2.Daemon;
using System.Collections.Generic;

namespace BD2.Test.Daemon.FileShare
{
	class MainClass
	{
		public static object ConsoleLock = new object ();

		public static string ConsoleReadLine ()
		{
			//Console.WriteLine (new System.Diagnostics.StackTrace ().ToString ());
			lock (ConsoleLock)
				return Console.ReadLine ();
		}

		static void HandleConnection (string modeName, Guid fileShareServiceAnouncementType, System.Net.Sockets.TcpClient TC)
		{
			System.IO.Stream PS = TC.GetStream ();
			StreamHandler SH = new StreamHandler (PS);
			ObjectBus OB = new ObjectBus (SH);
			ServiceManager SM = new ServiceManager (OB);
			if (modeName == "Server") {
				SM.AnnounceService (new ServiceAnnounceMessage (Guid.NewGuid (), fileShareServiceAnouncementType, "FileShare"), FileShareAgent.CreateAgent);
			}
			if (modeName == "Client") {
				System.Threading.Thread.Sleep (100);//wait for service announcement from remote
				SortedSet<ServiceAnnounceMessage> RSAs = SM.EnumerateRemoteServices ();
				foreach (ServiceAnnounceMessage RSA in RSAs) {
					Console.WriteLine ("Service found: {0}", RSA.Name);
					Console.WriteLine ("Press Enter to request service");
					ConsoleReadLine ();
					SM.RequestService (RSA, FileShareAgent.CreateAgent);
				}
			}
		}

		public static void Main (string[] args)
		{
			Console.Write ("Please Enter Operation Mode <Client|Server>: ");
			string modeName = ConsoleReadLine ();

			Guid fileShareServiceAnouncementType = Guid.Parse ("d44568fe-2bbb-4e4b-8aa8-3fb07dc86178");
			if (modeName == "Server") {
				System.Net.Sockets.TcpListener TL = new System.Net.Sockets.TcpListener (new System.Net.IPEndPoint (System.Net.IPAddress.Parse ("0.0.0.0"), 28000));
				TL.Start ();
				while (true)
					HandleConnection (modeName, fileShareServiceAnouncementType, TL.AcceptTcpClient ());
			}
			if (modeName == "Client") {
				System.Net.Sockets.TcpClient TC = null;
				TC = new System.Net.Sockets.TcpClient ();
				Console.Write ("Please Enter Remote IP: ");
				TC.Connect (new System.Net.IPEndPoint (System.Net.IPAddress.Parse (ConsoleReadLine ()), 28000));
				HandleConnection (modeName, fileShareServiceAnouncementType, TC);
			}
		}
	}
}
