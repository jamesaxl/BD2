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
	[ObjectBusMessageTypeIDAttribute("8cb84d5a-d5c2-4a2f-a1e9-adc4498baefe")]
	[ObjectBusMessageDeserializerAttribute(typeof(TransparentStreamCanSeekRequestMessage), "Deserialize")]
	sealed class TransparentStreamCanSeekRequestMessage : TransparentStreamMessageBase
	{
		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		public TransparentStreamCanSeekRequestMessage (Guid id, Guid streamID)
		{
			this.id = id;
			this.streamID = streamID;
		}

		public static TransparentStreamCanSeekRequestMessage Deserialize (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			Guid id;
			Guid streamID;
			using (System.IO.MemoryStream MS =  new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader(MS)) {
					id = new Guid (BR.ReadBytes (16));
					streamID = new Guid (BR.ReadBytes (16));
				}
			}
			return new TransparentStreamCanSeekRequestMessage (id, streamID);
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS  =new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW =  new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (streamID.ToByteArray ());
				}
				return MS.ToArray ();
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("8cb84d5a-d5c2-4a2f-a1e9-adc4498baefe");
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

