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

namespace BD2.Test.Daemon.FileShare
{
	class MainClass
	{
		public static object ConsoleLock = new object ();

		public static string ConsoleReadLine ()
		{
#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace ().ToString ());
#endif
			lock (ConsoleLock)
				return Console.ReadLine ();
		}

		static void HandleConnection (string modeName, Guid fileShareServiceAnouncementType, System.Net.Sockets.TcpClient TC)
		{
			if (modeName == null)
				throw new ArgumentNullException ("modeName");
			if (TC == null)
				throw new ArgumentNullException ("TC");
			System.IO.Stream PS = TC.GetStream ();
			StreamHandler SH = new StreamHandler (PS);
			ObjectBus OB = new ObjectBus (SH);
			ServiceManager SM = new ServiceManager (OB);
			if (modeName == "Server") {
				SM.AnnounceService (new ServiceAnnounceMessage (Guid.NewGuid (), fileShareServiceAnouncementType, "FileShare"), FileShareAgent.CreateAgent);
			}
			if (modeName == "Client") {
				foreach (ServiceAnnounceMessage RSA in SM.EnumerateRemoteServices ()) {
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
