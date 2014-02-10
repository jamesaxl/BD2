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

namespace BD2.Daemon
{
	sealed class TransparentStreamServer
	{
		int maximumSatisfiableUnit;
		Guid streamID;
		ServiceAgent agent;
		System.IO.Stream baseStream;
		System.Collections.Concurrent.ConcurrentQueue<TransparentStreamMessageBase> pendingRequests;
		System.Threading.ManualResetEvent queueWaitHandle;
		System.Threading.Thread threadProcessQueue;

		public void EnqueueRequest (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			pendingRequests.Enqueue (transparentStreamMessageBase);
			queueWaitHandle.Set ();
		}

		public TransparentStreamServer (ServiceAgent agent, System.IO.Stream storageBackend)
			:this(agent, storageBackend, short.MaxValue)
		{

		}

		public TransparentStreamServer (ServiceAgent agent, System.IO.Stream baseStream, int maximumSatisfiableUnit)
		{
			if (agent == null)
				throw new ArgumentNullException ("agent");
			if (baseStream == null)
				throw new ArgumentNullException ("baseStream");
			if (maximumSatisfiableUnit < 1)
				throw new ArgumentOutOfRangeException ("maximumSatisfiableUnit");
			this.maximumSatisfiableUnit = maximumSatisfiableUnit;
			this.streamID = Guid.NewGuid ();
			this.agent = agent;
			this.baseStream = baseStream;
			threadProcessQueue = new System.Threading.Thread (ProcessQueue);
			threadProcessQueue.Start ();
		}

		void ProcessQueue ()
		{
			bool Done = false;
			do {
				TransparentStreamMessageBase transparentStreamMessageBase;
				while (pendingRequests.TryDequeue (out transparentStreamMessageBase)) {
					if (transparentStreamMessageBase is TransparentStreamCloseRequestMessage) {
						Done = TransparentStreamCloseRequestMessageReceived (transparentStreamMessageBase);
					} else if (transparentStreamMessageBase is TransparentStreamFlushRequestMessage) {
						TransparentStreamFlushRequestMessageReceived (transparentStreamMessageBase);
					}
				}
			} while(!Done);
		}

		public ServiceAgent Agent {
			get {
				return agent;
			}
		}

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		public int MaximumSatisfiableUnit {
			get {
				return maximumSatisfiableUnit;
			}
		}
		#region "Callbacks"
		bool TransparentStreamCloseRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			Guid requestID = Guid.Empty;
			try {
				TransparentStreamCloseRequestMessage request = (TransparentStreamCloseRequestMessage)transparentStreamMessageBase;
				requestID = request.ID;
				baseStream.Close ();
				TransparentStreamCloseResponseMessage response = new TransparentStreamCloseResponseMessage (streamID, requestID, null);
				agent.ObjectBusSession.SendMessage (response);
				return true;
			} catch (Exception ex) {
				TransparentStreamCloseResponseMessage response = new TransparentStreamCloseResponseMessage (streamID, requestID, ex);
				agent.ObjectBusSession.SendMessage (response);
				return false;
			}
		}

		void TransparentStreamFlushRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamGetLengthRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamGetReadTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamGetWriteTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamReadRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamSetLengthRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamSetReadTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamSetWriteTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}

		void TransparentStreamWriteRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
		}
		#endregion
	}
}
