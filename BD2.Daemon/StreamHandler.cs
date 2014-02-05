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
using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;

namespace BD2.Daemon
{
	/// <summary>
	/// Daemon class can be used to serve any connection, remote or otherwise
	/// </summary>
	public class StreamHandler
	{
	
		Stream peer;
		ConcurrentQueue<byte[]> sendQueue = new ConcurrentQueue<byte[]> ();
		System.Threading.Thread thread_tx, thread_rx;
		List<Action<byte[]>> callbacks = new List<Action<byte[]>> ();
		List<Action<StreamHandler>> disconnectCallbacks = new  List<Action<StreamHandler>> ();

		public void RegisterCallback (Action<byte[]> callback)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			lock (callbacks) {
				callbacks.Add (callback);
			}
		}

		public void RegisterDisconnectHandler (Action<StreamHandler> callback)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			lock (disconnectCallbacks)
				disconnectCallbacks.Add (callback);
		}

		public void SendMessage (byte[] messageContents)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (!thread_tx.IsAlive)
				throw new InvalidOperationException ("thread_tx is not alive.");
			sendQueue.Enqueue (messageContents);
		}

		public StreamHandler (Stream peer)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (peer == null)
				throw new ArgumentNullException ("peer");
			this.peer = peer;
		}

		public void Start ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			thread_tx = new System.Threading.Thread (tx);
			thread_rx = new System.Threading.Thread (rx);
			thread_rx.Start ();
			thread_tx.Start ();
			while (!thread_tx.IsAlive)
				System.Threading.Thread.Sleep (0);
			while (!thread_rx.IsAlive)
				System.Threading.Thread.Sleep (0);
		}

		void PeerDisconnected ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			Action<StreamHandler>[] dcbs;
			lock (disconnectCallbacks) {
				dcbs = disconnectCallbacks.ToArray ();
			}
			foreach (Action<StreamHandler> dcb in dcbs)
				dcb (this);
		}

		void tx ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			BinaryWriter peerWriter = new BinaryWriter (peer);
			while (thread_rx.IsAlive) {
				byte[] messageBytes;
					
				while (!sendQueue.TryDequeue (out messageBytes))
					System.Threading.Thread.Sleep (0);
				try {
					WriteMessage (peerWriter, messageBytes);
				} catch {
					PeerDisconnected ();
					return;
				}
			}
		}

		void rx ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			BinaryReader peerReader = new BinaryReader (peer);
			for (bool alive = true; alive;) {
				byte[] messageBytes;
				try {
					messageBytes = ReadMessage (peerReader);
				} catch {
					PeerDisconnected ();
					return;
				}
				if (messageBytes.Length == 0) {
					alive = false;
					continue;
				}
				Action<byte[]>[] cbs;
				lock (callbacks) {
					cbs = callbacks.ToArray ();
				}
				foreach (Action<byte[]> cb in cbs)
					cb (messageBytes);
			}
		}

		static byte[] ReadMessage (BinaryReader reader)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (reader == null)
				throw new ArgumentNullException ("reader");
			return reader.ReadBytes (reader.ReadInt32 ());
		}

		static void WriteMessage (BinaryWriter writer, params byte[][] bytes)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			int totalBytes = 0;
			for (int n = 0; n != bytes.Length; n++)
				totalBytes += bytes [n].Length;
			writer.Write (totalBytes);
			for (int n = 0; n != bytes.Length; n++)
				writer.Write (bytes [n]);
		}

		public void Flush ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			peer.Flush ();
		}
	}
}

