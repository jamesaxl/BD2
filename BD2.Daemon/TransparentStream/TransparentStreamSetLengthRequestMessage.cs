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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
using System;

namespace BD2.Daemon
{
	[ObjectBusMessageTypeIDAttribute("053dd45d-6455-49d0-9f3d-07ccc9b2fe6f")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamSetLengthRequestMessage), "Deserialize")]
	sealed class TransparentStreamSetLengthRequestMessage : TransparentStreamMessageBase
	{
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		Guid streamID;

		public override Guid StreamID {
			get {
				return streamID;
			}
		}

		long length;

		public long Length {
			get {
				return length;
			}
		}

		public TransparentStreamSetLengthRequestMessage (Guid id, Guid streamID, long length)
		{
			this.id = id;
			this.streamID = streamID;
			this.length = length;
		}

		public static TransparentStreamSetLengthRequestMessage Deserialize (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			Guid id;
			Guid streamID;
			long length;
			using (System.IO.MemoryStream MS =  new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader(MS)) {
					id = new Guid (BR.ReadBytes (16));
					streamID = new Guid (BR.ReadBytes (16));
					length = BR.ReadInt64 ();
				}
			}
			return new TransparentStreamSetLengthRequestMessage (id, streamID, length);
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
					BW.Write (length);
					return MS.ToArray ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("053dd45d-6455-49d0-9f3d-07ccc9b2fe6f");
			}
		}
		#endregion
	}
}

