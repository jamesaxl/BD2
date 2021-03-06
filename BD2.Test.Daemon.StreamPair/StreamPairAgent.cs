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
using BD2.Daemon;
using System.IO;
using BD2.Daemon.Buses;

namespace BD2.Test.Daemon.StreamPair
{
	public class StreamPairAgent : ServiceAgent
	{

		StreamPairAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, bool run)
			: base (serviceAgentMode, objectBusSession, flush, run)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			objectBusSession.RegisterType (typeof(StreamPairMessage), StreamPairMessageReceived);
		}

		public static ServiceAgent CreateAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, byte[] parameters)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			return new StreamPairAgent (serviceAgentMode, objectBusSession, flush, true);
		}

		void StreamPairMessageReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			StreamPairMessage StreamPairMessage = (StreamPairMessage)message;
			Stream stream = OpenStream (StreamPairMessage.StreamID);
			System.IO.BinaryReader BR = new BinaryReader (stream);
			byte[] buf = BR.ReadBytes (BR.ReadInt32 ());
			byte[] hash = System.Security.Cryptography.SHA1.Create ().ComputeHash (buf);
			for (int n = 0; n != hash.Length; n++)
				Console.Write ("{0:X2}", hash [n]);
			Console.WriteLine ();

		}

		void SendStreamPairMessage (Stream stream)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			ObjectBusSession.SendMessage (new StreamPairMessage (CreateStream (stream)));
		}

		#region implemented abstract members of ServiceAgent

		static void SendObject (BinaryWriter bw, string path)
		{
			byte[] buf = System.IO.File.ReadAllBytes (path);
			bw.Write (buf.Length);
			bw.Write (buf);
			byte[] hash = System.Security.Cryptography.SHA1.Create ().ComputeHash (buf);
			for (int n = 0; n != hash.Length; n++)
				Console.Write ("{0:X2}", hash [n]);
			Console.WriteLine ();
		}

		protected override void Run ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			ObjectBusSession.AnounceReady ();
			ObjectBusSession.WaitForRemoteReady ();
			try {

				DateTime t1 = DateTime.UtcNow;
				BD2.Daemon.StreamPair SP = new BD2.Daemon.StreamPair ();
				SendStreamPairMessage (SP.GetOStream ());
				System.IO.BinaryWriter BW = new BinaryWriter (SP.GetIStream ());
				SendObject (BW, "/home/behrooz/The unix haters handbook.pdf");
				SendObject (BW, "/home/behrooz/john_1.7.8.orig.tar.gz");
				SendObject (BW, "/home/behrooz/Flag_Register_of_80386.PNG");
				SendObject (BW, "/home/behrooz/BD2.Arch.odg");

				Flush ();
				Console.WriteLine ("Done in {0}", (DateTime.UtcNow - t1).TotalMilliseconds);
				//Destroy ();
			} catch (System.Threading.ThreadAbortException) {
			}
		}

		protected override void DestroyRequestReceived ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			Thread.Abort ();
		}

		protected override void SessionDisconnected ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			DestroyRequestReceived ();
		}

		#endregion
	}
}

