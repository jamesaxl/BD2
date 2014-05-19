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
using System.Collections.Generic;

namespace BD2.Daemon
{
	public sealed class ObjectBus
	{
		SortedDictionary<Guid, ObjectBusSession> sessions = new SortedDictionary<Guid, ObjectBusSession> ();
		SortedDictionary<ObjectBusSession, Action<byte[]>> streamHandlerCallbackHandlers = new SortedDictionary<ObjectBusSession, Action<byte[]>> ();
		StreamHandler streamHandler;
		ObjectBusSession systemSession;

		public void RegisterType (Type type, Action<ObjectBusMessage> action)
		{
			systemSession.RegisterType (type, action);
		}

		public void SendMessage (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			systemSession.SendMessage (message);
		}

		void SendMessageHandler (ObjectBusMessage message, ObjectBusSession session)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			byte[] messageBody = message.GetMessageBody ();
			byte[] bytes = new byte[32 + messageBody.Length]; 
			System.Buffer.BlockCopy (session.SessionID.ToByteArray (), 0, bytes, 0, 16);
			System.Buffer.BlockCopy (message.TypeID.ToByteArray (), 0, bytes, 16, 16);
			System.Buffer.BlockCopy (messageBody, 0, bytes, 32, messageBody.Length);
			Console.WriteLine ("Sending a {0} byte {1} on bus", bytes.Length, message.GetType ());
			streamHandler.SendMessage (bytes);
		}

		class tpmessage
		{
			byte[] message;
			Action<byte[]> cb;

			public tpmessage (Action<byte[]> cb, byte[] message)
			{
				this.message = message;
				this.cb = cb;
			}

			public void tpcallback (object tc)
			{
				cb (message);
			}
		}

		void StreamHandlerCallback (byte[] messageContents)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (messageContents == null)
				throw new ArgumentNullException ("messageContents");
			if (messageContents.Length < 32)
				throw new ArgumentException ("messageContents cannot be less than 16 bytes.");
			byte[] sessionIDBytes = new byte[16];
			System.Buffer.BlockCopy (messageContents, 0, sessionIDBytes, 0, 16);
			Guid sessionID = new Guid (sessionIDBytes);
			ObjectBusSession session = null;
			Console.WriteLine ("session id={0}", sessionID);
			if (sessionID == Guid.Empty)
				session = systemSession;
			else {
				foreach (var tup in sessions) {
					Console.WriteLine ("we have {0}", tup.Key);
				}
				lock (sessions)
					session = sessions [sessionID];
			}
			byte[] bytes = new byte[messageContents.Length - 16];
			System.Buffer.BlockCopy (messageContents, 16, bytes, 0, messageContents.Length - 16);
			System.Threading.ThreadPool.QueueUserWorkItem ((new tpmessage (streamHandlerCallbackHandlers [session], bytes)).tpcallback);
		}

		public ObjectBus (StreamHandler streamHandler)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (streamHandler == null)
				throw new ArgumentNullException ("streamHandler");
			this.streamHandler = streamHandler;
			streamHandler.RegisterCallback (StreamHandlerCallback);
			streamHandler.RegisterDisconnectHandler (streamHandlerDisconnected);
			systemSession = CreateSession (Guid.Empty, SystemSessionDisconnected);
			RegisterType (typeof(BusReadyMessage), bunReadyMessageReceived);
		}

		void bunReadyMessageReceived (ObjectBusMessage message)
		{
			BusReadyMessage brm = (BusReadyMessage)message;
			sessions [brm.ObjectBusSessionID].setRemoteReady ();
		}

		void streamHandlerDisconnected (StreamHandler streamHandler)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			foreach (var session in sessions)
				session.Value.BusDisconnected ();
			systemSession.BusDisconnected ();
		}

		void SystemSessionDisconnected (ObjectBusSession session)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (systemSession != session)
				throw new InvalidOperationException ();
			//do nothing?
		}

		public void Start ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			streamHandler.Start ();
		}

		public void Flush ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			streamHandler.Flush ();
		}

		public ObjectBusSession CreateSession (Guid sessionID, Action<ObjectBusSession> sessionDisconnected)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			ObjectBusSession session = new ObjectBusSession (sessionID, SendMessageHandler, RegisterStreamHandlerCallbackHandler, DestroyHandler, sessionDisconnected, sessionReady);
			lock (sessions)
				sessions.Add (sessionID, session);
			return session;
		}

		void sessionReady (ObjectBusSession session)
		{
			SendMessage (new BusReadyMessage (session.SessionID));
		}

		void RegisterStreamHandlerCallbackHandler (Action<byte[]> streamHandlerCallbackHandler, ObjectBusSession session)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			lock (streamHandlerCallbackHandlers)
				streamHandlerCallbackHandlers.Add (session, streamHandlerCallbackHandler);
		}

		public void DestroySession (ServiceDestroyMessage serviceDestroy)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (serviceDestroy == null)
				throw new ArgumentNullException ("serviceDestroy");
			ObjectBusSession session;
			lock (sessions) {
				//TODO:remove this line
				if (!sessions.ContainsKey (serviceDestroy.SessionID))
					return;
				session = sessions [serviceDestroy.SessionID];
				if (session == systemSession) {
					if (sessions.Count != 0) {
						throw new InvalidOperationException ("System session must be the last session to be destroyed.");
					}
				}
				sessions.Remove (serviceDestroy.SessionID);
			}
		}

		void DestroyHandler (ObjectBusSession session)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			ServiceDestroyMessage serviceDestroy = new ServiceDestroyMessage (session.SessionID);
			SendMessage (serviceDestroy);
			DestroySession (serviceDestroy);
		}
	}
}
