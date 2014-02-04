using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("43a54d04-52b8-4052-94bf-bd1512995dec")]
	[ObjectBusMessageDeserializerAttribute(typeof(ServiceDestroy), "Deserialize")]
	public class ServiceDestroy : ObjectBusMessage
	{
		Guid sessionID;

		public Guid SessionID {
			get {
				return sessionID;
			}
		}

		public ServiceDestroy (Guid sessionID)
		{
			this.sessionID = sessionID;
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new ServiceDestroy (new Guid (BR.ReadBytes (16)));
				}
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (sessionID.ToByteArray ());
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("43a54d04-52b8-4052-94bf-bd1512995dec");
			}
		}
		#endregion
	}
}

