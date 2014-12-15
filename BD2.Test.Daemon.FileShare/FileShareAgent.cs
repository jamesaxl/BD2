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

namespace BD2.Test.Daemon.FileShare
{
	public class FileShareAgent : ServiceAgent
	{
		System.Threading.ManualResetEvent mre_fileReceived = new System.Threading.ManualResetEvent (false);
		System.Collections.Concurrent.ConcurrentQueue<FileShareMessage> files = new System.Collections.Concurrent.ConcurrentQueue<FileShareMessage> ();

		FileShareAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, bool run)
			: base (serviceAgentMode, objectBusSession, flush, run)
		{
			objectBusSession.RegisterType (typeof(FileShareMessage), FileShareMessageReceived);
		}

		public static ServiceAgent CreateAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, byte[] parameters)
		{
			return new FileShareAgent (serviceAgentMode, objectBusSession, flush, true);
		}

		void FileShareMessageReceived (ObjectBusMessage message)
		{
			FileShareMessage fileShareMessace = (FileShareMessage)message;
			files.Enqueue (fileShareMessace);
			mre_fileReceived.Set ();
		}

		void SendMessage (string text)
		{
			ObjectBusSession.SendMessage (new FileShareMessage (text, CreateStream (File.Open (text, FileMode.Open))));
		}

		#region implemented abstract members of ServiceAgent

		protected override void Run ()
		{
			try {
				if (ServiceAgentMode == ServiceAgentMode.Server) {
					while (true) {
						Console.Write ("path for file to share: ");
						string p = MainClass.ConsoleReadLine ();
						if (p == "done")
							return;
						SendMessage (p);
					}
				}
				while (true) {
					FileShareMessage fileShareMessage;
					mre_fileReceived.WaitOne (10);
					while (files.TryDequeue (out fileShareMessage)) {
						mre_fileReceived.Reset ();
						Console.Write ("server has shared a file: ");
						Console.WriteLine (fileShareMessage.Text);
						using (TransparentStream s = OpenStream (fileShareMessage.StreamID)) {
							Console.Write ("path to save: ");
							s.CopyTo (File.OpenWrite (MainClass.ConsoleReadLine ()), 64, 512);	
						}
					}
				}
			} catch (System.Threading.ThreadAbortException) {
			}
		}

		protected override void DestroyRequestReceived ()
		{
			Thread.Abort ();
		}

		protected override void SessionDisconnected ()
		{
			Thread.Abort ();
		}

		#endregion
	}
}

