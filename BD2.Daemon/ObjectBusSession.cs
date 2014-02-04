using System;
using System.Collections.Generic;

namespace BD2.Daemon
{
	public sealed class ObjectBusSession : IComparable
	{
		Guid sessionID;

		public Guid SessionID{ get { return sessionID; } }

		Action<ObjectBusMessage,  ObjectBusSession> sendMessageCallback;
		Action<ObjectBusSession> destroyCallback;
		Action<ObjectBusSession> busDisconnected;
		SortedDictionary<string, Action<ObjectBusMessage>> callbacks = new SortedDictionary<string, Action<ObjectBusMessage>> ();
		SortedDictionary<Guid, ObjectBusMessageDeserializerAttribute> deserializers = new SortedDictionary<Guid, ObjectBusMessageDeserializerAttribute> ();

		public void RegisterType (Type type, Action<ObjectBusMessage> action)
		{
			ObjectBusMessageDeserializerAttribute[] procAttribs = (ObjectBusMessageDeserializerAttribute[])type.GetCustomAttributes (typeof(ObjectBusMessageDeserializerAttribute), false);
			ObjectBusMessageTypeIDAttribute[] idAttribs = (ObjectBusMessageTypeIDAttribute[])type.GetCustomAttributes (typeof(ObjectBusMessageTypeIDAttribute), false);
			lock (deserializers) {
				deserializers.Add (idAttribs [0].ObjectTypeID, procAttribs [0]);
			}
			lock (callbacks)
				callbacks.Add (type.FullName, action);
		}

		public void SendMessage (ObjectBusMessage message)
		{
			sendMessageCallback (message, this);
		}

		void streamHandlerCallback (byte[] messageContents)
		{
			if (messageContents == null)
				throw new ArgumentNullException ("messageContents");
			if (messageContents.Length < 16)
				throw new ArgumentException ("messageContents cannot be less than 16 bytes.");
			byte[] messageTypeBytes = new byte[16];
			System.Buffer.BlockCopy (messageContents, 0, messageTypeBytes, 0, 16);
			Guid MessageType = new Guid (messageTypeBytes);
			ObjectBusMessageDeserializerAttribute obmda = deserializers [MessageType];
			byte[] bytes = new byte[messageContents.Length - 16];
			System.Buffer.BlockCopy (messageContents, 16, bytes, 0, messageContents.Length - 16);
			ObjectBusMessage messageObject = obmda.Deserialize (bytes);
			lock (callbacks)
				foreach (var ct in callbacks) {
					if (ct.Key == messageObject.GetType ().ToString ()) {
						ct.Value (messageObject);
					}
				}
		}

		internal ObjectBusSession (Guid sessionID, Action<ObjectBusMessage, ObjectBusSession> sendMessageCallback, Action<Action<byte[]>, ObjectBusSession> registerStreamCallbackCallback, Action<ObjectBusSession> destroyCallback, Action<ObjectBusSession> busDisconnectedCallback)
		{
			this.sessionID = sessionID;
			this.sendMessageCallback = sendMessageCallback;
			this.destroyCallback = destroyCallback;
			this.busDisconnected = busDisconnectedCallback;
			registerStreamCallbackCallback (streamHandlerCallback, this);
		}

		public void Destroy ()
		{
			destroyCallback (this);
		}
		#region IComparable implementation
		public int CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			return sessionID.CompareTo ((obj as ObjectBusSession).sessionID);
		}
		#endregion
		internal void BusDisconnected ()
		{
			busDisconnected (this);
		}
	}
}

