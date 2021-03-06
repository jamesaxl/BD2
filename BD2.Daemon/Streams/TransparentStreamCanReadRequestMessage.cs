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

namespace BD2.Daemon.Streams
{
	[ObjectBusMessageTypeIDAttribute ("8d5373ac-3b2b-42b5-8473-9db3cdead652")]
	[ObjectBusMessageDeserializerAttribute (typeof(TransparentStreamCanReadRequestMessage), "Deserialize")]
	sealed class TransparentStreamCanReadRequestMessage : TransparentStreamMessageBase
	{
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		public TransparentStreamCanReadRequestMessage (Guid id, Guid streamID)
		{
			this.id = id;
			this.streamID = streamID;
		}

		public static TransparentStreamCanReadRequestMessage Deserialize (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			Guid id;
			Guid streamID;
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					id = new Guid (BR.ReadBytes (16));
					streamID = new Guid (BR.ReadBytes (16));
				}
			}
			return new TransparentStreamCanReadRequestMessage (id, streamID);
		}

		#region implemented abstract members of ObjectBusMessage

		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
				}
				return MS.ToArray ();
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("8d5373ac-3b2b-42b5-8473-9db3cdead652");
			}
		}

		#endregion

		#region implemented abstract members of TransparentStreamMessageBase

		Guid streamID;

		public override Guid StreamID {
			get {
				return streamID;
			}
		}

		#endregion
	}
}

