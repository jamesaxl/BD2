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
using System.Collections.Concurrent;
using BD2.Daemon.Streams;
using BD2.Daemon.Buses;

namespace BD2.Daemon
{
	public abstract class ServiceAgent
	{
		ConcurrentDictionary<Guid, TransparentStreamServer> streamServers = new  ConcurrentDictionary<Guid, TransparentStreamServer> ();
		ConcurrentDictionary<Guid, TransparentStream> streams = new ConcurrentDictionary<Guid, TransparentStream> ();
		ServiceAgentMode serviceAgentMode;
		ObjectBusSession objectBusSession;

		protected Guid CreateStream (System.IO.Stream backendStream)
		{
			TransparentStreamServer tss = new TransparentStreamServer (this, backendStream, objectBusSession);
			streamServers.TryAdd (tss.StreamID, tss);
			return tss.StreamID;
		}

		protected void DestroyStream (Guid index)
		{
			TransparentStreamServer tss;
			streamServers.TryRemove (index, out tss);
		}

		protected TransparentStream OpenStream (Guid streamID)
		{
			TransparentStream ts = new TransparentStream (this, streamID, objectBusSession);
			streams.TryAdd (streamID, ts);
			return ts;
		}

		internal void RemoveStream (TransparentStream transparentStream)
		{
			TransparentStream referenece;
			streams.TryGetValue (transparentStream.StreamID, out referenece);
			if (referenece != transparentStream) {
				throw new InvalidOperationException ("requested stream has an id belonging to this agent but the object itself does not");
			}
			streams.TryRemove (transparentStream.StreamID, out referenece);
		}

		internal void RemoveStreamServer (TransparentStreamServer transparentStreamServer)
		{
			TransparentStreamServer referenece;
			streamServers.TryGetValue (transparentStreamServer.StreamID, out referenece);
			if (referenece != transparentStreamServer) {
				throw new InvalidOperationException ("requested stream server has an id belonging to this agent but the object itself does not");
			}
			streamServers.TryRemove (transparentStreamServer.StreamID, out referenece);
		}

		protected ServiceAgentMode ServiceAgentMode {
			get {
				return serviceAgentMode;
			}
		}

		protected ObjectBusSession ObjectBusSession {
			get {
				return objectBusSession;
			}
		}

		System.Threading.Thread thread;

		protected System.Threading.Thread Thread {
			get {
				return thread;
			}
		}

		Action flush;

		protected void Flush ()
		{
			flush ();
		}

		protected ServiceAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, bool run)
		{
			if (!Enum.IsDefined (typeof(ServiceAgentMode), serviceAgentMode)) {
				throw new ArgumentException ("Invalid value for argument 'serviceAgentMode'", "serviceAgentMode");
			}
			if (objectBusSession == null)
				throw new ArgumentNullException ("objectBusSession");
			if (flush == null)
				throw new ArgumentNullException ("flush");
			this.serviceAgentMode = serviceAgentMode;
			this.objectBusSession = objectBusSession;
			this.flush = flush;
			objectBusSession.RegisterType (typeof(TransparentStreamCanReadRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanReadResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanSeekRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanSeekResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanTimeoutRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanTimeoutResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanWriteRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCanWriteResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCloseRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamCloseResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamFlushRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamFlushResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetLengthRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetLengthResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetPositionRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetPositionResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetReadTimeoutRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetReadTimeoutResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetWriteTimeoutRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamGetWriteTimeoutResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamReadRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamReadResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSeekRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSeekResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetLengthRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetLengthResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetPositionRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetPositionResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetReadTimeoutRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetReadTimeoutResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetWriteTimeoutRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamSetWriteTimeoutResponseMessage), TransparentStreamMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamWriteRequestMessage), TransparentStreamServerMessageReceived);
			objectBusSession.RegisterType (typeof(TransparentStreamWriteResponseMessage), TransparentStreamMessageReceived);
			if (run) {
				thread = new System.Threading.Thread (Run);
				thread.Start ();
			}
		}

		public void Destroy ()
		{
			ObjectBusSession.Destroy ();
		}

		protected virtual void Run ()
		{
		}

		internal void CallDestroyRequestReceived ()
		{
			DestroyRequestReceived ();
		}

		protected virtual void DestroyRequestReceived ()
		{
		}

		internal void CallSessionDisconnected ()
		{
			SessionDisconnected ();
		}

		protected virtual void SessionDisconnected ()
		{
		}

		void TransparentStreamServerMessageReceived (ObjectBusMessage objectBusMessage)
		{
			if (objectBusMessage == null)
				throw new ArgumentNullException ("objectBusMessage");
			TransparentStreamMessageBase transparentStreamMessageBase = objectBusMessage as TransparentStreamMessageBase;
			if (transparentStreamMessageBase == null)
				throw new ArgumentException ("objectBusMessage must be of type TransparentStreamMessageBase", "objectBusMessage");
			streamServers [transparentStreamMessageBase.StreamID].EnqueueRequest (transparentStreamMessageBase);
		}

		void TransparentStreamMessageReceived (ObjectBusMessage objectBusMessage)
		{
			if (objectBusMessage == null)
				throw new ArgumentNullException ("objectBusMessage");
			TransparentStreamMessageBase transparentStreamMessageBase = objectBusMessage as TransparentStreamMessageBase;
			if (transparentStreamMessageBase == null)
				throw new ArgumentException ("objectBusMessage must be of type TransparentStreamMessageBase", "objectBusMessage");
			streams [transparentStreamMessageBase.StreamID].ResponseReceived (transparentStreamMessageBase);
		}
	}
}
