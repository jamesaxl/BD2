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
using BD2.Daemon.Buses;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute ("51aecfc5-8e82-4ca8-af1d-c5c556d23a55")]
	[ObjectBusMessageDeserializerAttribute (typeof(ServiceAnnounceMessage), "Deserialize")]
	public sealed class ServiceAnnounceMessage : ObjectBusMessage, IComparable
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

		byte[] meta;

		public ServiceAnnounceMessage (Guid id, Guid type, string name, byte[] meta)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.id = id;
			this.type = type;
			this.name = name;
			this.meta = meta;
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					Guid id = new Guid (BR.ReadBytes (16));
					Guid typeID = new Guid (BR.ReadBytes (16));
					string name = BR.ReadString ();
					byte[] meta = BR.ReadBytes (BR.ReadInt32 ());

					return new ServiceAnnounceMessage (id, typeID, name, meta);
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
					BW.Write (meta.Length);
					BW.Write (meta);
					return MS.ToArray ();
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
			return id.CompareTo ((obj as ServiceAnnounceMessage).id);
		}

		#endregion
	}
}

