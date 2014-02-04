using System;
using BD2.Daemon;

namespace BD2.Test.Daemon.Chat
{
	public class ChatAgent:ServiceAgent
	{

		ChatAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession)
			:base(serviceAgentMode, objectBusSession)
		{
			objectBusSession.RegisterType (typeof(ChatMessage), ChatMessageReceived);
		}

		public static ServiceAgent CreateAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession)
		{
			return new ChatAgent (serviceAgentMode, objectBusSession);
		}

		void ChatMessageReceived (ObjectBusMessage message)
		{
			ChatMessage chatMessace = (ChatMessage)message;
			Console.WriteLine (chatMessace.Text);
		}

		void SendMessage (string text)
		{
			ObjectBusSession.SendMessage (new ChatMessage (text));
		}
		#region implemented abstract members of ServiceAgent
		protected override void Run ()
		{
			try {
				if (ServiceAgentMode == ServiceAgentMode.Server) {
					return;
				}
				string nextMessage = MainClass.ConsoleReadLine ();
				while (nextMessage != "/destroy") {
					SendMessage (nextMessage);
					nextMessage = MainClass.ConsoleReadLine ();
				}
				Destroy ();
			} catch (System.Threading.ThreadAbortException) {
			}
		}

		public override void DestroyRequestReceived ()
		{
			Thread.Abort ();
		}

		public override void SessionDisconnected ()
		{
			DestroyRequestReceived ();
		}
		#endregion
	}
}

