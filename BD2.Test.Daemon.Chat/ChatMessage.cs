using System;
using BD2.Daemon;

namespace BD2.Test.Daemon.Chat
{
	[ObjectBusMessageTypeIDAttribute("b4f471fa-f56c-44b0-88b8-714a3ab3427b")]
	[ObjectBusMessageDeserializerAttribute(typeof(ChatMessage), "Deserialize")]

	public class ChatMessage : ObjectBusMessage
	{

		string text;

		public string Text {
			get {
				return text;
			}
		}

		public ChatMessage (string text)
		{
			this.text = text;
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new ChatMessage (BR.ReadString ());
				}
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (text);
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("b4f471fa-f56c-44b0-88b8-714a3ab3427b");
			}
		}
		#endregion
	}
}

