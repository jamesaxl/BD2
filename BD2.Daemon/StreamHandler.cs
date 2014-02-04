using System;
using System.Collections.Generic;
using System.IO;

namespace BD2.Daemon
{
	/// <summary>
	/// Daemon class can be used to serve any connection, remote or otherwise
	/// </summary>
	public class StreamHandler
	{
		Stream peer;
		Queue<byte[]> sendQueue = new Queue<byte[]> ();
		System.Threading.Thread thread_tx, thread_rx;
		System.Threading.ManualResetEventSlim mresTx = new System.Threading.ManualResetEventSlim (false);
		List<Action<byte[]>> callbacks = new List<Action<byte[]>> ();
		List<Action<StreamHandler>> disconnectCallbacks = new  List<Action<StreamHandler>> ();

		public void RegisterCallback (Action<byte[]> callback)
		{
			lock (callbacks) {
				callbacks.Add (callback);
			}
		}

		public void RegisterDisconnectHandler (Action<StreamHandler> callback)
		{
			disconnectCallbacks.Add (callback);
		}

		public void SendMessage (byte[] messageContents)
		{
			if (!thread_tx.IsAlive)
				throw new InvalidOperationException ();
			lock (sendQueue) {
				sendQueue.Enqueue (messageContents);
				mresTx.Set ();
			}
		}

		public StreamHandler (Stream peer)
		{
			if (peer == null)
				throw new ArgumentNullException ("peer");
			this.peer = peer;
		}

		public void Start ()
		{
			thread_tx = new System.Threading.Thread (tx);
			thread_rx = new System.Threading.Thread (rx);
			thread_rx.Start ();
			thread_tx.Start ();
		}

		void tx ()
		{
			BinaryWriter peerWriter = new BinaryWriter (peer);
			while (thread_rx.IsAlive) {
				mresTx.Wait ();
				byte[] messageBytes;
				lock (sendQueue) {
					if (sendQueue.Count == 0) {
						mresTx.Reset ();
						continue;
					} else
						messageBytes = sendQueue.Dequeue ();
				}
				try {
					WriteMessage (peerWriter, messageBytes);
				} catch {
					Action<StreamHandler>[] dcbs;
					lock (disconnectCallbacks) {
						dcbs = disconnectCallbacks.ToArray ();
					}
					foreach (Action<StreamHandler> dcb in dcbs)
						dcb (this);
					return;
				}
			}
		}

		void rx ()
		{
			BinaryReader peerReader = new BinaryReader (peer);
			for (bool alive = true; alive;) {
				byte[] messageBytes;
				try {
					messageBytes = ReadMessage (peerReader);
				} catch {
					Action<StreamHandler>[] dcbs;
					lock (disconnectCallbacks) {
						dcbs = disconnectCallbacks.ToArray ();
					}
					foreach (Action<StreamHandler> dcb in dcbs)
						dcb (this);
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
			if (reader == null)
				throw new ArgumentNullException ("reader");
			return reader.ReadBytes (reader.ReadInt32 ());
		}

		static void WriteMessage (BinaryWriter writer, params byte[][] bytes)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			int totalBytes = 0;
			for (int n =0; n != bytes.Length; n++)
				totalBytes += bytes [n].Length;
			writer.Write (totalBytes);
			for (int n =0; n != bytes.Length; n++)
				writer.Write (bytes [n]);
		}
	}
}

