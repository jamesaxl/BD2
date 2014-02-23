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
		Guid streamID;
		ServiceAgent agent;
		System.IO.Stream baseStream;
		ObjectBusSession objectBusSession;

		public void EnqueueRequest (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			if (transparentStreamMessageBase == null)
				throw new ArgumentNullException ("transparentStreamMessageBase");
			if (transparentStreamMessageBase is TransparentStreamCanReadRequestMessage) {
				TransparentStreamCanReadRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCanSeekRequestMessage) {
				TransparentStreamCanSeekRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCanTimeoutRequestMessage) {
				TransparentStreamCanTimeoutRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCanWriteRequestMessage) {
				TransparentStreamCanWriteRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamCloseRequestMessage) {
				if (TransparentStreamCloseRequestMessageReceived (transparentStreamMessageBase))
					agent.RemoveStreamServer (this);
			} else if (transparentStreamMessageBase is TransparentStreamFlushRequestMessage) {
				TransparentStreamFlushRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetLengthRequestMessage) {
				TransparentStreamGetLengthRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetPositionRequestMessage) {
				TransparentStreamGetPositionRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetReadTimeoutRequestMessage) {
				TransparentStreamGetReadTimeoutRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamGetWriteTimeoutRequestMessage) {
				TransparentStreamGetWriteTimeoutRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamReadRequestMessage) {
				TransparentStreamReadRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSeekRequestMessage) {
				TransparentStreamSeekRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetLengthRequestMessage) {
				TransparentStreamSetLengthRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetPositionRequestMessage) {
				TransparentStreamSetPositionRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetReadTimeoutRequestMessage) {
				TransparentStreamSetReadTimeoutRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamSetWriteTimeoutRequestMessage) {
				TransparentStreamSetWriteTimeoutRequestMessageReceived (transparentStreamMessageBase);
			} else if (transparentStreamMessageBase is TransparentStreamWriteRequestMessage) {
				TransparentStreamWriteRequestMessageReceived (transparentStreamMessageBase);
			}
		}

		public TransparentStreamServer (ServiceAgent agent, System.IO.Stream baseStream, ObjectBusSession objectBusSession)
		{
			if (agent == null)
				throw new ArgumentNullException ("agent");
			if (baseStream == null)
				throw new ArgumentNullException ("baseStream");
			if (objectBusSession == null)
				throw new ArgumentNullException ("objectBusSession");
			this.streamID = Guid.NewGuid ();
			this.agent = agent;
			this.baseStream = baseStream;
			this.objectBusSession = objectBusSession;
			 
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
		#region "Callbacks"
		void TransparentStreamCanReadRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamCanReadRequestMessage request = (TransparentStreamCanReadRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			bool canRead = false;
			try {
				canRead = baseStream.CanRead;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamCanReadResponseMessage (streamID, request.ID, canRead, exception));
		}

		void TransparentStreamCanSeekRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamCanSeekRequestMessage request = (TransparentStreamCanSeekRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			bool canSeek = false;
			try {
				canSeek = baseStream.CanSeek;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamCanSeekResponseMessage (streamID, request.ID, canSeek, exception));
		}

		void TransparentStreamCanTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamCanTimeoutRequestMessage request = (TransparentStreamCanTimeoutRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			bool canTimeout = false;
			try {
				canTimeout = baseStream.CanTimeout;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamCanTimeoutResponseMessage (streamID, request.ID, canTimeout, exception));
		}

		void TransparentStreamCanWriteRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamCanWriteRequestMessage request = (TransparentStreamCanWriteRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			bool canWrite = false;
			try {
				canWrite = baseStream.CanWrite;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamCanWriteResponseMessage (streamID, request.ID, canWrite, exception));
		}

		bool TransparentStreamCloseRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamCloseRequestMessage request = (TransparentStreamCloseRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.Close ();
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamCloseResponseMessage (streamID, request.ID, exception));
			return (exception == null);
		}

		void TransparentStreamFlushRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamFlushRequestMessage request = (TransparentStreamFlushRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.Flush ();
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamFlushResponseMessage (streamID, request.ID, exception));
		}

		void TransparentStreamGetLengthRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamGetLengthRequestMessage request = (TransparentStreamGetLengthRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			long length = -1;
			try {
				length = baseStream.Length;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamGetLengthResponseMessage (streamID, request.ID, length, exception));
		}

		void TransparentStreamGetPositionRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamGetPositionRequestMessage request = (TransparentStreamGetPositionRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			long position = -1;
			try {
				position = baseStream.Length;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamGetPositionResponseMessage (streamID, request.ID, position, exception));
		}

		void TransparentStreamGetReadTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamGetReadTimeoutRequestMessage request = (TransparentStreamGetReadTimeoutRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			int readTimeout = -1;
			try {
				readTimeout = baseStream.ReadTimeout;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamGetReadTimeoutResponseMessage (streamID, request.ID, readTimeout, exception));
		}

		void TransparentStreamGetWriteTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamGetWriteTimeoutRequestMessage request = (TransparentStreamGetWriteTimeoutRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			int writeTimeout = -1;
			try {
				writeTimeout = baseStream.WriteTimeout;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamGetWriteTimeoutResponseMessage (streamID, request.ID, writeTimeout, exception));
		}

		void TransparentStreamReadRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamReadRequestMessage request = (TransparentStreamReadRequestMessage)transparentStreamMessageBase;
			byte[] buffer = new byte[request.Count];
			try {
				baseStream.BeginRead (buffer, 0, request.Count, (ar) =>
				{
					try {
						int bytesRead = baseStream.EndRead (ar);
						if (bytesRead != ((byte[])ar.AsyncState).Length) {
							byte[] newBuffer = new byte[bytesRead];
							System.Buffer.BlockCopy ((byte[])ar.AsyncState, 0, newBuffer, 0, bytesRead);
							objectBusSession.SendMessage (new TransparentStreamReadResponseMessage (streamID, request.ID, newBuffer, null));
						} else {
							objectBusSession.SendMessage (new TransparentStreamReadResponseMessage (streamID, request.ID, (byte[])ar.AsyncState, null));
						}
					} catch (Exception ex) {
						objectBusSession.SendMessage (new TransparentStreamReadResponseMessage (streamID, request.ID, (byte[])ar.AsyncState, ex));
					}
				}, buffer);
			} catch (Exception ex) {
				objectBusSession.SendMessage (new TransparentStreamReadResponseMessage (streamID, request.ID, null, ex));
			}
		}

		void TransparentStreamSeekRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamSeekRequestMessage request = (TransparentStreamSeekRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			long seek = -1;
			try {
				seek = baseStream.Seek (request.Offset, request.SeekOrigin);
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamSeekResponseMessage (streamID, request.ID, seek, exception));
		}

		void TransparentStreamSetLengthRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamSetLengthRequestMessage request = (TransparentStreamSetLengthRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.SetLength (request.Length);
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamSetLengthResponseMessage (streamID, request.ID, exception));
		}

		void TransparentStreamSetPositionRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamSetPositionRequestMessage request = (TransparentStreamSetPositionRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.Position = request.Position;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamSetPositionResponseMessage (streamID, request.ID, exception));
		}

		void TransparentStreamSetReadTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamSetReadTimeoutRequestMessage request = (TransparentStreamSetReadTimeoutRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.ReadTimeout = request.ReadTimeout;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamSetReadTimeoutResponseMessage (streamID, request.ID, exception));
		}

		void TransparentStreamSetWriteTimeoutRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamSetWriteTimeoutRequestMessage request = (TransparentStreamSetWriteTimeoutRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.WriteTimeout = request.WriteTimeout;
			} catch (Exception ex) {
				exception = ex;
			}
			objectBusSession.SendMessage (new TransparentStreamSetWriteTimeoutResponseMessage (streamID, request.ID, exception));
		}

		void TransparentStreamWriteRequestMessageReceived (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			TransparentStreamWriteRequestMessage request = (TransparentStreamWriteRequestMessage)transparentStreamMessageBase;
			Exception exception = null;
			try {
				baseStream.BeginWrite (request.Buffer, 0, request.Buffer.Length, (ar) => {
					try {
						baseStream.EndWrite (ar);
					} catch (Exception ex) {
						objectBusSession.SendMessage (new TransparentStreamWriteResponseMessage (streamID, request.ID, ex));
					}
				}, request.Buffer);
			} catch (Exception ex) {
				exception = ex;
				objectBusSession.SendMessage (new TransparentStreamWriteResponseMessage (streamID, request.ID, exception));
			}
		}
		#endregion
	}
}
