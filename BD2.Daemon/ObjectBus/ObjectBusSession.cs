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
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

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
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			sendMessageCallback (message, this);
		}

		void streamHandlerCallback (byte[] messageContents)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			if (messageContents == null)
				throw new ArgumentNullException ("messageContents");
			if (messageContents.Length < 16)
				throw new ArgumentException ("messageContents cannot be less than 16 bytes.");
			byte[] messageTypeBytes = new byte[16];
			System.Buffer.BlockCopy (messageContents, 0, messageTypeBytes, 0, 16);
			Guid MessageType = new Guid (messageTypeBytes);
			if (!deserializers.ContainsKey (MessageType)) {
				throw new Exception (string.Format ("Deserializer for object type id '{0}' is not registered", MessageType));
			}
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
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			this.sessionID = sessionID;
			this.sendMessageCallback = sendMessageCallback;
			this.destroyCallback = destroyCallback;
			this.busDisconnected = busDisconnectedCallback;
			registerStreamCallbackCallback (streamHandlerCallback, this);
		}

		public void Destroy ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

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
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			busDisconnected (this);
		}
	}
}

