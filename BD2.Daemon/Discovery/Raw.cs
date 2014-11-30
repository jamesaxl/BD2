// /*
//  * Copyright (c) 2014 Behrooz Amoozad
//  * All rights reserved.
//  *
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the bd2 nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  *
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  * */
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace BD2.Daemon.Discovery
{
	public class Raw
	{
		public int BroadcastInterval = 10000;
		readonly Thread rxThread, txThread;
		readonly int groupPort;
		Action<Tuple<IPEndPoint, byte[]>> rxCallback;
		Func<byte[]> txCallback;

		public Raw (int groupPort)
		{
			this.groupPort = groupPort;
			rxThread = new Thread (Listen);
			txThread = new Thread (Send);
		}

		/// <summary>
		/// Sets the receive callback.
		/// </summary>
		/// <param name="rxCallback">Rx callback, it better be fast or we lose messages</param>
		public void SetReceiveCallback (Action<Tuple<IPEndPoint, byte[]>> rxCallback)
		{
			if (rxCallback == null)
				throw new ArgumentNullException ("rxCallback");
			this.rxCallback = rxCallback;
			if (rxThread.ThreadState == ThreadState.Unstarted)
				rxThread.Start ();
		}

		void Listen ()
		{
			UdpClient udp = new UdpClient (groupPort);
			while (rxCallback != null) {
				IPEndPoint remoteEP = new IPEndPoint (IPAddress.Any, 0);
				try {
					byte[] receiveBytes = udp.Receive (ref remoteEP);
					rxCallback (new Tuple<IPEndPoint, byte[]> (remoteEP, receiveBytes));
				} catch (Exception ex) {
					Console.Error.WriteLine (ex.Message);
				}
			}
		}

		public void SetTransmitCallback (Func<byte[]> txCallback)
		{
			if (txCallback == null)
				throw new ArgumentNullException ("txCallback");
			this.txCallback = txCallback;
			if (txThread.ThreadState == ThreadState.Unstarted)
				txThread.Start ();

		}

		short txTTL = 2;

		public short TxTTL {
			get {
				return txTTL;
			}
			set {
				txTTL = value;
			}
		}

		void Send ()
		{
			UdpClient udp = new UdpClient ();
			udp.Connect (new IPEndPoint (IPAddress.Broadcast, groupPort));
			udp.EnableBroadcast = true;
			while (txCallback != null) {
				try {
					udp.Ttl = txTTL;
					byte[] message = txCallback ();
					udp.Send (message, message.Length);
				} catch (Exception ex) {
					Console.Error.WriteLine (ex.Message);
				}
				Thread.Sleep (BroadcastInterval);
			}
		}


		public void Stop ()
		{
			txCallback = null;
			rxCallback = null;
		}


		public void Terminate ()
		{
			if (rxThread.IsAlive)
				rxThread.Abort ();
			if (txThread.IsAlive)
				txThread.Abort ();
		}

		public bool StillWorking { get { return rxThread.IsAlive || txThread.IsAlive; } }

	}
}
