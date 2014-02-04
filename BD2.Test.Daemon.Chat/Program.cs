using System;
using BD2.Daemon;
using System.Collections.Generic;

namespace BD2.Test.Daemon.Chat
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

		static void HandleConnection (string modeName, Guid ChatServiceAnouncementType, System.Net.Sockets.TcpClient TC)
		{
			System.IO.Stream PS = TC.GetStream ();
			StreamHandler SH = new StreamHandler (PS);
			ObjectBus OB = new ObjectBus (SH);
			ServiceManager SM = new ServiceManager (OB);
			if (modeName == "Server") {
				SM.AnnounceService (new ServiceAnnouncement (Guid.NewGuid (), ChatServiceAnouncementType, "Chat"), ChatAgent.CreateAgent);
			}
			if (modeName == "Client") {
				bool AnyAgent = false;
				while (!AnyAgent) {
					SortedSet<ServiceAnnouncement> RSAs = SM.EnumerateRemoteServices ();
					foreach (ServiceAnnouncement RSA in RSAs) {
						Console.WriteLine ("Service found: {0}", RSA.Name);
						Console.WriteLine ("Enter Request to request service");
						if (ConsoleReadLine () == "Request") {
							SM.RequestService (RSA, ChatAgent.CreateAgent);
							AnyAgent = true;

						}
						//ServiceAgent agent = new  ServiceAgent (mode, OB, SR, new Type[] { typeof(ChatMessage) });
					}
				}
			}
		}

		public static void Main (string[] args)
		{
			Console.Write ("Please Enter Operation Mode <Client|Server>: ");
			string modeName = ConsoleReadLine ();

			Guid ChatServiceAnouncementType = Guid.Parse ("b0021151-a1cc-4f82-aa8d-a2cdb905e6ca");
			if (modeName == "Server") {
				System.Net.Sockets.TcpListener TL = new System.Net.Sockets.TcpListener (new System.Net.IPEndPoint (System.Net.IPAddress.Parse ("0.0.0.0"), 28000));
				TL.Start ();
				while (true)
					HandleConnection (modeName, ChatServiceAnouncementType, TL.AcceptTcpClient ());
			}
			if (modeName == "Client") {
				System.Net.Sockets.TcpClient TC = null;
				TC = new System.Net.Sockets.TcpClient ();
				Console.Write ("Please Enter Remote IP: ");
				TC.Connect (new System.Net.IPEndPoint (System.Net.IPAddress.Parse (ConsoleReadLine ()), 28000));
				HandleConnection (modeName, ChatServiceAnouncementType, TC);
			}
		}
	}
}
