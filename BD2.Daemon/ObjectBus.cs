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
			systemSession.SendMessage (message);
		}

		void SendMessageHandler (ObjectBusMessage message, ObjectBusSession session)
		{
			byte[] messageBody = message.GetMessageBody ();
			byte[] bytes = new byte[32 + messageBody.Length]; 
			System.Buffer.BlockCopy (session.SessionID.ToByteArray (), 0, bytes, 0, 16);
			System.Buffer.BlockCopy (message.TypeID.ToByteArray (), 0, bytes, 16, 16);
			System.Buffer.BlockCopy (messageBody, 0, bytes, 32, messageBody.Length);
			streamHandler.SendMessage (bytes);
		}

		void StreamHandlerCallback (byte[] messageContents)
		{
			if (messageContents == null)
				throw new ArgumentNullException ("messageContents");
			if (messageContents.Length < 32)
				throw new ArgumentException ("messageContents cannot be less than 16 bytes.");
			byte[] sessionIDBytes = new byte[16];
			System.Buffer.BlockCopy (messageContents, 0, sessionIDBytes, 0, 16);
			Guid sessionID = new Guid (sessionIDBytes);
			ObjectBusSession session = null;
			if (sessionID == Guid.Empty)
				session = systemSession;
			else
				session = sessions [sessionID];
			byte[] bytes = new byte[messageContents.Length - 16];
			System.Buffer.BlockCopy (messageContents, 16, bytes, 0, messageContents.Length - 16);
			streamHandlerCallbackHandlers [session] (bytes);
		}

		public ObjectBus (StreamHandler streamHandler)
		{
			if (streamHandler == null)
				throw new ArgumentNullException ("streamHandler");
			this.streamHandler = streamHandler;
			streamHandler.RegisterCallback (StreamHandlerCallback);
			streamHandler.RegisterDisconnectHandler (streamHandlerDisconnected);
			systemSession = CreateSession (Guid.Empty, SystemSessionDisconnected);
		}

		void streamHandlerDisconnected (StreamHandler streamHandler)
		{
			foreach (var session in sessions)
				session.Value.BusDisconnected ();
			systemSession.BusDisconnected ();
		}

		void SystemSessionDisconnected (ObjectBusSession session)
		{
			if (systemSession != session)
				throw new InvalidOperationException ();
			//do nothing?
		}

		public void Start ()
		{
			streamHandler.Start ();
		}

		public ObjectBusSession CreateSession (Guid sessionID, Action<ObjectBusSession> sessionDisconnected)
		{
			ObjectBusSession session = new ObjectBusSession (sessionID, SendMessageHandler, RegisterStreamHandlerCallbackHandler, DestroyHandler, sessionDisconnected);
			sessions.Add (sessionID, session);
			return session;
		}

		void RegisterStreamHandlerCallbackHandler (Action<byte[]> streamHandlerCallbackHandler, ObjectBusSession session)
		{
			lock (streamHandlerCallbackHandlers)
				streamHandlerCallbackHandlers.Add (session, streamHandlerCallbackHandler);
		}

		public void DestroySession (ServiceDestroy serviceDestroy)
		{
			if (serviceDestroy == null)
				throw new ArgumentNullException ("serviceDestroy");
			ObjectBusSession session = sessions [serviceDestroy.SessionID];
			if (session == systemSession) {
				if (sessions.Count != 0) {
					throw new InvalidOperationException ("System session must be the last session to be destroyed.");
				}
			}
			lock (sessions)
				sessions.Remove (session.SessionID);
			lock (streamHandlerCallbackHandlers)
				streamHandlerCallbackHandlers.Remove (session);
		}

		void DestroyHandler (ObjectBusSession session)
		{
			ServiceDestroy serviceDestroy = new ServiceDestroy (session.SessionID);
			DestroySession (serviceDestroy);
			SendMessage (serviceDestroy);
		}
	}
}
