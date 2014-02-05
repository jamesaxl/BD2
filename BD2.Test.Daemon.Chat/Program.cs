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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
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

		static void HandleConnection (string modeName, Guid chatServiceAnouncementType, System.Net.Sockets.TcpClient TC)
		{
			System.IO.Stream PS = TC.GetStream ();
			StreamHandler SH = new StreamHandler (PS);
			ObjectBus OB = new ObjectBus (SH);
			ServiceManager SM = new ServiceManager (OB);
			if (modeName == "Server") {
				SM.AnnounceService (new ServiceAnnouncement (Guid.NewGuid (), chatServiceAnouncementType, "Chat"), ChatAgent.CreateAgent);
			}
			if (modeName == "Client") {
				System.Threading.Thread.Sleep (100);//wait for service announcement from remote
				SortedSet<ServiceAnnouncement> RSAs = SM.EnumerateRemoteServices ();
				foreach (ServiceAnnouncement RSA in RSAs) {
					Console.WriteLine ("Service found: {0}", RSA.Name);
					Console.WriteLine ("Press Enter to request service");
					ConsoleReadLine ();
					SM.RequestService (RSA, ChatAgent.CreateAgent);
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
