using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("51aecfc5-8e82-4ca8-af1d-c5c556d23a55")]
	[ObjectBusMessageDeserializerAttribute(typeof(ServiceAnnouncement), "Deserialize")]
	public sealed class ServiceAnnouncement : ObjectBusMessage, IComparable
	{
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		Guid type;

		public Guid Type {
			get {
				return type;
			}
		}

		string name;

		public string Name {
			get {
				return name;
			}
		}

		public ServiceAnnouncement (Guid id, Guid type, string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.id = id;
			this.type = type;
			this.name = name;
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new ServiceAnnouncement (new Guid (BR.ReadBytes (16)), new Guid (BR.ReadBytes (16)), BR.ReadString ());
				}
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (type.ToByteArray ());
					BW.Write (name);
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("51aecfc5-8e82-4ca8-af1d-c5c556d23a55");
			}
		}
		#endregion
		#region IComparable implementation
		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			return id.CompareTo ((obj as ServiceAnnouncement).id);
		}
		#endregion
	}
}

