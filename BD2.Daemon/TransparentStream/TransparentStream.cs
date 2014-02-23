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
using System.Collections.Concurrent;

namespace BD2.Daemon
{
	public sealed class TransparentStream : System.IO.Stream
	{
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingCanReadRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingCanSeekRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingCanTimeoutRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingCanWriteRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingCloseRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingWriteRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingFlushRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingGetLengthRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingGetPositionRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingGetReadTimeoutRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingSeekRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingGetWriteTimeoutRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingSetLengthRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingSetPositionRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, Tuple <TransparentStreamAsyncResult, byte[], int, int>> pendingReadRequests =
			new ConcurrentDictionary<Guid, Tuple <TransparentStreamAsyncResult, byte[], int, int>> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingSetReadTimeoutRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ConcurrentDictionary<Guid, TransparentStreamAsyncResult> pendingSetWriteTimeoutRequests =
			new ConcurrentDictionary<Guid, TransparentStreamAsyncResult> ();
		ServiceAgent agent;

		public ServiceAgent Agent {
			get {
				return agent;
			}
		}

		Guid streamID;

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		ObjectBusSession objectBusSession;

		public ObjectBusSession ObjectBusSession {
			get {
				return objectBusSession;
			}
		}

		internal TransparentStream (ServiceAgent agent, Guid streamID, ObjectBusSession objectBusSession)
		{
			if (agent == null)
				throw new ArgumentNullException ("agent");
			if (objectBusSession == null)
				throw new ArgumentNullException ("objectBusSession");
			this.agent = agent;
			this.streamID = streamID;
			this.objectBusSession = objectBusSession;
		}
		#region implemented abstract members of Stream
		public override void Flush ()
		{
			EndFlush (BeginFlush ());
		}

		public IAsyncResult BeginFlush ()
		{
			TransparentStreamFlushRequestMessage request = new TransparentStreamFlushRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingFlushRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public void EndFlush (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamFlushResponseMessage response = (TransparentStreamFlushResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return EndRead (BeginRead (buffer, offset, count, null, null));
		}

		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			return EndSeek (BeginSeek (offset, origin));
		}

		public IAsyncResult BeginSeek (long offset, System.IO.SeekOrigin origin)
		{
			TransparentStreamSeekRequestMessage request = new TransparentStreamSeekRequestMessage (Guid.NewGuid (), streamID, offset, origin);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingSeekRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public long EndSeek (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamSeekResponseMessage response = (TransparentStreamSeekResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.Seek;
		}

		public override void SetLength (long value)
		{
			EndSetLength (BeginSetLength (value));
		}

		public IAsyncResult BeginSetLength (long value)
		{
			TransparentStreamSetLengthRequestMessage request = new TransparentStreamSetLengthRequestMessage (Guid.NewGuid (), streamID, value);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingSetLengthRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public void EndSetLength (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamSetLengthResponseMessage response = (TransparentStreamSetLengthResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			EndWrite (BeginWrite (buffer, offset, count, null, null));
		}

		public override bool CanRead {
			get {
				return EndCanRead (BeginCanRead ());
			}
		}

		public IAsyncResult BeginCanRead ()
		{
			TransparentStreamCanReadRequestMessage request = new  TransparentStreamCanReadRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingCanReadRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public bool EndCanRead (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamCanReadResponseMessage response = (TransparentStreamCanReadResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.CanRead;
		}

		public override bool CanSeek {
			get {
				return EndCanSeek (BeginCanSeek ());
			}
		}

		public IAsyncResult BeginCanSeek ()
		{
			TransparentStreamCanSeekRequestMessage request = new  TransparentStreamCanSeekRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingCanSeekRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public bool EndCanSeek (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamCanSeekResponseMessage response = (TransparentStreamCanSeekResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.CanSeek;
		}

		public override bool CanWrite {
			get {
				return EndCanWrite (BeginCanWrite ());
			}
		}

		public IAsyncResult BeginCanWrite ()
		{
			TransparentStreamCanWriteRequestMessage request = new  TransparentStreamCanWriteRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingCanWriteRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public bool EndCanWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamCanWriteResponseMessage response = (TransparentStreamCanWriteResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.CanWrite;
		}

		public override long Length {
			get {
				return EndGetLength (BeginGetLength ());
			}
		}

		public IAsyncResult BeginGetLength ()
		{
			TransparentStreamGetLengthRequestMessage request = new  TransparentStreamGetLengthRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingGetLengthRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public long EndGetLength (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamGetLengthResponseMessage response = (TransparentStreamGetLengthResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.Length;
		}

		public override long Position {
			get {
				return EndGetPosition (BeginGetPosition ());
			}
			set {
				EndSetPosition (BeginSetPosition (value));
			}
		}

		public IAsyncResult BeginGetPosition ()
		{
			TransparentStreamGetPositionRequestMessage request = new TransparentStreamGetPositionRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingGetPositionRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public long EndGetPosition (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamGetPositionResponseMessage response = (TransparentStreamGetPositionResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.Position;
		}

		public IAsyncResult BeginSetPosition (long value)
		{
			TransparentStreamSetPositionRequestMessage request = new TransparentStreamSetPositionRequestMessage (Guid.NewGuid (), streamID, value);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingSetPositionRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public void EndSetPosition (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamSetPositionResponseMessage response = (TransparentStreamSetPositionResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}
		#endregion
		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			TransparentStreamReadRequestMessage request = new TransparentStreamReadRequestMessage (Guid.NewGuid (), streamID, count);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (callback, state);
			if (!pendingReadRequests.TryAdd (request.ID, new Tuple <TransparentStreamAsyncResult, byte[], int, int> (result, buffer, offset, count))) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			byte[] nbuf = new byte[count];
			System.Buffer.BlockCopy (buffer, offset, nbuf, 0, count);
			TransparentStreamWriteRequestMessage request = new TransparentStreamWriteRequestMessage (Guid.NewGuid (), streamID, nbuf);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingWriteRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamReadResponseMessage response = (TransparentStreamReadResponseMessage)result.Response;
			Tuple <TransparentStreamAsyncResult, byte[], int, int> rtuple;
			pendingReadRequests.TryRemove (response.RequestID, out rtuple);
			if (response.Exception != null)
				throw response.Exception;
			if (rtuple.Item4 < response.Data.Length)
				throw new Exception ("Something is inherently wrong with remote stream.");
			Buffer.BlockCopy (response.Data, 0, rtuple.Item2, rtuple.Item3, Math.Min (rtuple.Item4, response.Data.Length));
			return response.Data.Length;
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamWriteResponseMessage response = (TransparentStreamWriteResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public override int ReadTimeout {
			get {
				return EndGetReadTimeout (BeginGetReadTimeout ());
			}
			set {
				EndSetReadTimeout (BeginSetReadTimeout (value));
			}
		}

		public override int WriteTimeout {
			get {
				return EndGetWriteTimeout (BeginGetWriteTimeout ());
			}
			set {
				EndSetWriteTimeout (BeginSetWriteTimeout (value));
			}
		}

		public IAsyncResult BeginGetWriteTimeout ()
		{
			TransparentStreamGetWriteTimeoutRequestMessage request = new TransparentStreamGetWriteTimeoutRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingGetWriteTimeoutRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public int EndGetWriteTimeout (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamGetWriteTimeoutResponseMessage response = (TransparentStreamGetWriteTimeoutResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.WriteTimeout;
		}

		public IAsyncResult BeginSetWriteTimeout (int value)
		{
			TransparentStreamSetWriteTimeoutRequestMessage request = new TransparentStreamSetWriteTimeoutRequestMessage (Guid.NewGuid (), streamID, value);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingSetWriteTimeoutRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public void EndSetWriteTimeout (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamSetWriteTimeoutResponseMessage response = (TransparentStreamSetWriteTimeoutResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}

		public IAsyncResult BeginGetReadTimeout ()
		{
			TransparentStreamGetReadTimeoutRequestMessage request = new TransparentStreamGetReadTimeoutRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingGetReadTimeoutRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public int EndGetReadTimeout (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamGetReadTimeoutResponseMessage response = (TransparentStreamGetReadTimeoutResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.ReadTimeout;
		}

		public IAsyncResult BeginSetReadTimeout (int value)
		{
			TransparentStreamSetReadTimeoutRequestMessage request = new TransparentStreamSetReadTimeoutRequestMessage (Guid.NewGuid (), streamID, value);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingSetReadTimeoutRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public void EndSetReadTimeout (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamSetReadTimeoutResponseMessage response = (TransparentStreamSetReadTimeoutResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}

		public override bool CanTimeout {
			get {
				return EndCanTimeout (BeginCanTimeout ());
			}
		}

		public IAsyncResult BeginCanTimeout ()
		{
			TransparentStreamCanTimeoutRequestMessage request = new TransparentStreamCanTimeoutRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingCanTimeoutRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public bool EndCanTimeout (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			TransparentStreamCanTimeoutResponseMessage response = (TransparentStreamCanTimeoutResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
			return response.CanTimeout;
		}

		public override void Close ()
		{
			EndClose (BeginClose ());
		}

		public IAsyncResult BeginClose ()
		{
			TransparentStreamCloseRequestMessage request = new TransparentStreamCloseRequestMessage (Guid.NewGuid (), streamID);
			TransparentStreamAsyncResult result = new TransparentStreamAsyncResult (null);
			if (!pendingCloseRequests.TryAdd (request.ID, result)) {
				throw new Exception ("request failed before sending.");
			}
			objectBusSession.SendMessage (request);
			return result;
		}

		public void EndClose (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			if (!(asyncResult is TransparentStreamAsyncResult)) {
				throw new ArgumentException ("asyncResult must be of type TransparentStreamAsyncResult.");
			}
			TransparentStreamAsyncResult result = (TransparentStreamAsyncResult)asyncResult;
			if (result.Response == null)
				Console.WriteLine ("No response!!!");
			TransparentStreamCloseResponseMessage response = (TransparentStreamCloseResponseMessage)result.Response;
			if (response.Exception != null)
				throw response.Exception;
		}

		internal void ResponseReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase is TransparentStreamCanReadResponseMessage) {
				TransparentStreamCanReadResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCanSeekResponseMessage) {
				TransparentStreamCanSeekResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCanTimeoutResponseMessage) {
				TransparentStreamCanTimeoutResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCanWriteResponseMessage) {
				TransparentStreamCanWriteResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCloseResponseMessage) {
				TransparentStreamCloseResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamFlushResponseMessage) {
				TransparentStreamFlushResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetLengthResponseMessage) {
				TransparentStreamGetLengthResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetPositionResponseMessage) {
				TransparentStreamGetPositionResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetReadTimeoutResponseMessage) {
				TransparentStreamGetReadTimeoutResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetWriteTimeoutResponseMessage) {
				TransparentStreamGetWriteTimeoutResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamReadResponseMessage) {
				TransparentStreamReadResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSeekResponseMessage) {
				TransparentStreamSeekResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetLengthResponseMessage) {
				TransparentStreamSetLengthResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetPositionResponseMessage) {
				TransparentStreamSetPositionResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetReadTimeoutResponseMessage) {
				TransparentStreamSetReadTimeoutResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetWriteTimeoutResponseMessage) {
				TransparentStreamSetWriteTimeoutResponseMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamWriteResponseMessage) {
				TransparentStreamWriteResponseMessageReceived (transparentStreamMessageBase);
			}
		}
		#region "Callbacks"
		void TransparentStreamCanReadResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamCanReadResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamCanReadResponseMessage.");
			}
			TransparentStreamCanReadResponseMessage response = (TransparentStreamCanReadResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingCanReadRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamCanSeekResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamCanSeekResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamCanSeekResponseMessage.");
			}
			TransparentStreamCanSeekResponseMessage response = (TransparentStreamCanSeekResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingCanSeekRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamCanTimeoutResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamCanTimeoutResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamCanTimeoutResponseMessage.");
			}
			TransparentStreamCanTimeoutResponseMessage response = (TransparentStreamCanTimeoutResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingCanTimeoutRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamCanWriteResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamCanWriteResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamCanWriteResponseMessage.");
			}
			TransparentStreamCanWriteResponseMessage response = (TransparentStreamCanWriteResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingCanWriteRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamCloseResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamCloseResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamCloseResponseMessage.");
			}
			TransparentStreamCloseResponseMessage response = (TransparentStreamCloseResponseMessage)transparentStreamMessageBase;
			if (response.Exception != null) {
				agent.RemoveStream (this);
			}
			TransparentStreamAsyncResult ar;
			pendingCloseRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamFlushResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamFlushResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamFlushResponseMessage.");
			}
			TransparentStreamFlushResponseMessage response = (TransparentStreamFlushResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingFlushRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamGetLengthResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamGetLengthResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamGetLengthResponseMessage.");
			}
			TransparentStreamGetLengthResponseMessage response = (TransparentStreamGetLengthResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingGetLengthRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamGetPositionResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamGetPositionResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamGetPositionResponseMessage.");
			}
			TransparentStreamGetPositionResponseMessage response = (TransparentStreamGetPositionResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingGetPositionRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamGetReadTimeoutResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamGetReadTimeoutResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamGetReadTimeoutResponseMessage.");
			}
			TransparentStreamGetReadTimeoutResponseMessage response = (TransparentStreamGetReadTimeoutResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingGetReadTimeoutRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamGetWriteTimeoutResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamGetWriteTimeoutResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamGetWriteTimeoutResponseMessage.");
			}
			TransparentStreamGetWriteTimeoutResponseMessage response = (TransparentStreamGetWriteTimeoutResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingGetWriteTimeoutRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamReadResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamReadResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamReadResponseMessage.");
			}
			TransparentStreamReadResponseMessage response = (TransparentStreamReadResponseMessage)transparentStreamMessageBase;
			Tuple <TransparentStreamAsyncResult, byte[], int, int> tuple;
			pendingReadRequests.TryGetValue (response.RequestID, out tuple);
			tuple.Item1.Set (response);
		}

		void TransparentStreamSeekResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamSeekResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamSeekResponseMessage.");
			}
			TransparentStreamSeekResponseMessage response = (TransparentStreamSeekResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingSeekRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamSetLengthResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamSetLengthResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamSetLengthResponseMessage.");
			}
			TransparentStreamSetLengthResponseMessage response = (TransparentStreamSetLengthResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingSetLengthRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamSetPositionResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamSetPositionResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamSetPositionResponseMessage.");
			}
			TransparentStreamSetPositionResponseMessage response = (TransparentStreamSetPositionResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingSetPositionRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamSetReadTimeoutResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamSetReadTimeoutResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamSetReadTimeoutResponseMessage.");
			}
			TransparentStreamSetReadTimeoutResponseMessage response = (TransparentStreamSetReadTimeoutResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingSetReadTimeoutRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamSetWriteTimeoutResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamSetWriteTimeoutResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamSetWriteTimeoutResponseMessage.");
			}
			TransparentStreamSetWriteTimeoutResponseMessage response = (TransparentStreamSetWriteTimeoutResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingSetWriteTimeoutRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}

		void TransparentStreamWriteResponseMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase.StreamID != streamID)
				throw new Exception ("TransparentStreamMessageBase instance does not belong to this stream");
			if (!(transparentStreamMessageBase is TransparentStreamWriteResponseMessage)) {
				throw new ArgumentException ("transparentStreamMessageBase must be of type TransparentStreamWriteResponseMessage.");
			}
			TransparentStreamWriteResponseMessage response = (TransparentStreamWriteResponseMessage)transparentStreamMessageBase;
			TransparentStreamAsyncResult ar;
			pendingWriteRequests.TryRemove (response.RequestID, out ar);
			ar.Set (response);
		}
		#endregion
		public void CopyTo (System.IO.Stream destination, int minAwaits, int maxAwaits)
		{
			System.Collections.Generic.Queue<IAsyncResult> ars = new System.Collections.Generic.Queue<IAsyncResult> ();
			//todo: add automatic unit size tuning support
			int unit = 16384;
			long l;
			try {
				l = Length;
			} catch {
				l = long.MaxValue;
			}
			try {
				while (l!= 0) {
					int lt = (int)Math.Min (unit, l);
					if (l != long.MaxValue)
						l -= lt;
					byte[] bytes = new byte[lt];
					ars.Enqueue (BeginRead (bytes, 0, lt, (ar) => {
						int readbytes = EndRead (ar);
						byte[] buf = (byte[])ar.AsyncState;
						destination.BeginWrite (buf, 0, readbytes, (dar) => {
							destination.EndWrite (dar);
						}, null);
					}, bytes));
					if (ars.Count > maxAwaits) {
						while (ars.Count > minAwaits)
							ars.Dequeue ().AsyncWaitHandle.WaitOne ();
					}
				}
			} catch {
				//unknown length
				if (l != long.MaxValue)
					throw;
			}
			while (ars.Count != 0)
				ars.Dequeue ().AsyncWaitHandle.WaitOne ();
		}
	}
}