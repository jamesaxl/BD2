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
using BD2.Daemon;
using System.Collections.Generic;

namespace BD2.Chunk.Daemon.Common
{
	[ObjectBusMessageTypeIDAttribute("17eb68e2-43b5-4d43-b6af-6a2fd9ac1a25")]
	[ObjectBusMessageDeserializerAttribute(typeof(PollChunksRequestMessage), "Deserialize")]
	public class PollChunksRequestMessage : ObjectBusMessage
	{
		Guid id;
		bool requestData;
		Guid chunkIDs;

		public Guid ID {
			get {
				return id;
			}
		}

		public bool RequestData {
			get {
				return requestData;
			}
		}

		public PollChunksRequestMessage (Guid id, Guid chunkIDs, bool requestData)
		{
			this.id = id;
			this.requestData = requestData;
			this.chunkIDs = chunkIDs;
		}

		public static PollChunksRequestMessage Deserialize (byte[] bytes)
		{
			Guid id;
			Guid chunkIDs;
			bool requestData;
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					id = new Guid (BR.ReadBytes (16));
					chunkIDs = new Guid (BR.ReadBytes (16));
					requestData = BR.ReadBoolean ();
				}
			}
			return new PollChunksRequestMessage (id, chunkIDs, requestData);
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (chunkIDs.ToByteArray ());
					BW.Write (requestData);
				}
				return MS.ToArray ();
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("17eb68e2-43b5-4d43-b6af-6a2fd9ac1a25");
			}
		}
		#endregion
	}
}

