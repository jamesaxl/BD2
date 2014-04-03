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
using System.Collections.Generic;
using BD2.Daemon;

namespace BD2.Chunk.Daemon
{
	[ObjectBusMessageTypeIDAttribute("e71158b7-ee11-44a4-aac9-8f180871554c")]
	[ObjectBusMessageDeserializerAttribute(typeof(PushChunksRequestMessage), "Deserialize")]
	public class PushChunksRequestMessage : ObjectBusMessage
	{
		Guid id;
		Guid chunks;

		public PushChunksRequestMessage (Guid id, Guid chunks)
		{
			if (chunks == null)
				throw new ArgumentNullException ("chunkData");
			this.id = id;
			this.chunks = chunks;
		}

		public static PushChunksRequestMessage Deserialize (byte[] bytes)
		{
			Guid id;
			Guid chunks;
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					id = new Guid (BR.ReadBytes (16));
					chunks = new Guid (BR.ReadBytes (16));
				}
			}
			return new PushChunksRequestMessage (id, chunks);
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (chunks.ToByteArray ());
				}
				return MS.ToArray ();
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("e71158b7-ee11-44a4-aac9-8f180871554c");
			}
		}
		#endregion
	}
}

