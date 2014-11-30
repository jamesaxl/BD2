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
	[ObjectBusMessageTypeIDAttribute ("8aec7266-ebcc-454a-8958-2b03557fbc2e")]
	[ObjectBusMessageDeserializerAttribute (typeof(TransparentStreamSeekRequestMessage), "Deserialize")]
	sealed class TransparentStreamSeekRequestMessage : TransparentStreamMessageBase
	{
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		long offset;

		public long Offset {
			get {
				return offset;
			}
		}

		System.IO.SeekOrigin seekOrigin;

		public System.IO.SeekOrigin SeekOrigin {
			get {
				return seekOrigin;
			}
		}

		public TransparentStreamSeekRequestMessage (Guid id, Guid streamID, long offset, System.IO.SeekOrigin seekOrigin)
		{
			this.id = id;
			this.streamID = streamID;
			this.offset = offset;
			this.seekOrigin = seekOrigin;
		}

		public static TransparentStreamSeekRequestMessage Deserialize (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			Guid id;
			Guid streamID;
			long offset;
			System.IO.SeekOrigin seekOrigin;

			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					id = new Guid (BR.ReadBytes (16));
					streamID = new Guid (BR.ReadBytes (16));
					offset = BR.ReadInt64 ();
					seekOrigin = (System.IO.SeekOrigin)BR.ReadInt32 ();
				}
			}
			return new TransparentStreamSeekRequestMessage (id, streamID, offset, seekOrigin);
		}

		#region implemented abstract members of ObjectBusMessage

		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
					BW.Write (offset);
					BW.Write ((int)seekOrigin);
				}
				return MS.ToArray ();
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("8aec7266-ebcc-454a-8958-2b03557fbc2e");
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

