using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("e7002100-c14c-4170-9e0f-db54aea9a847")]
	[ObjectBusMessageDeserializerAttribute(typeof(ServiceResponse),"Deserialize")]
	public class ServiceResponse : ObjectBusMessage
	{
	
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		ServiceResponseStatus status;

		public ServiceResponseStatus Status {
			get {
				return status;
			}
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new ServiceResponse (new Guid (BR.ReadBytes (16)), new Guid (BR.ReadBytes (16)), (ServiceResponseStatus)BR.ReadInt32 ());
				}
			}
		}

		public ServiceResponse (Guid id, Guid requestID, ServiceResponseStatus status)
		{
			if (!Enum.IsDefined (typeof(ServiceResponseStatus), status)) {
				throw new ArgumentException ("Status is not valid", "status");
			}
			this.id = id;
			this.requestID = requestID;
			this.status = status;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (requestID.ToByteArray ());
					BW.Write ((int)status);
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("e7002100-c14c-4170-9e0f-db54aea9a847");
			}
		}
		#endregion
	}
}

