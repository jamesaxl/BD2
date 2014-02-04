using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("84210e38-8515-48e1-bc00-edbd595d757d")]
	[ObjectBusMessageDeserializerAttribute(typeof(ServiceRequest), "Deserialize")]
	public sealed class ServiceRequest : ObjectBusMessage
	{

		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		Guid serviceID;

		public Guid ServiceID {
			get {
				return serviceID;
			}
		}

		public ServiceRequest (Guid id, Guid serviceID)
		{
			this.id = id;
			this.serviceID = serviceID;
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new ServiceRequest (new Guid (BR.ReadBytes (16)), new Guid (BR.ReadBytes (16)));
				}
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (serviceID.ToByteArray ());
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("84210e38-8515-48e1-bc00-edbd595d757d");
			}
		}
		#endregion
	}
}

