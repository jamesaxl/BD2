using System;
using BD2.Common;
using System.Collections.Generic;

namespace BD2.Block.Daemon
{
	public class ObjectBus : BasicObjectBus
	{
		StreamHandler streamHandler;
		SortedDictionary<string, Action<MessageBusSession, Message>> callbacks;

		void streamHandlerCallback (byte[] messageContents)
		{
			Guid MessageType;

		}

		public ObjectBus (StreamHandler streamHandler)
		{
			if (streamHandler == null)
				throw new ArgumentNullException ("streamHandler");
			this.streamHandler = streamHandler;
			streamHandler.RegisterCallback (streamHandlerCallback);
		}
		#region implemented abstract members of BasicObjectMessageBus
		public override MessageBusSession OpenSession (string path, TimeSpan wait)
		{
			throw new NotImplementedException ();
		}

		public override void RequestService (MessageBusSession session, string path, Message message, Action<MessageBusSession, Message> callback)
		{
			throw new NotImplementedException ();
		}

		public override void AnounceService (string path, Action<MessageBusSession, Message> callback)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}

