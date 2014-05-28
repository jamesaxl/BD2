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
	[ObjectBusMessageTypeIDAttribute("eacfc35a-cc3d-4d2c-a27f-669dd41894ee")]
	[ObjectBusMessageDeserializerAttribute(typeof(TopLevelChunksRequestMessage), "Deserialize")]
	public class TopLevelChunksRequestMessage : ObjectBusMessage
	{

		Guid id;

		public Guid ID {
			get {
				return id;
			}
		}

		SortedSet<IRangedFilter> filters;

		public SortedSet<IRangedFilter> Filters {
			get {
				return new SortedSet<IRangedFilter> (filters);
			}
		}

		public TopLevelChunksRequestMessage (Guid id, SortedSet<IRangedFilter> filters)
		{
			if (filters == null)
				throw new ArgumentNullException ("filters");
			this.id = id;
			this.filters = filters;
		}

		public static ObjectBusMessage Deserialize (byte[]bytes)
		{

			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					Guid ID = new Guid (BR.ReadBytes (16));
					int FilterCount = BR.ReadInt32 ();
					SortedSet<IRangedFilter> filters = new  SortedSet<IRangedFilter> ();
					for (int n = 0; n != FilterCount; n++) {
						string FilterTypeName = BR.ReadString ();
						int filterLength = BR.ReadInt32 ();
						Func<byte[], IRangedFilter> deserializer;
						RangedFilterManager.GetFilterDeserializer (FilterTypeName, out deserializer);
						filters.Add (deserializer (BR.ReadBytes (filterLength)));
					}
					return new TopLevelChunksRequestMessage (ID, filters);
				}
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (id.ToByteArray ());
					BW.Write (filters.Count);
					foreach (IRangedFilter IRF in filters) {
						byte[] filter = IRF.GetMessageBody ();
						BW.Write (IRF.FilterTypeName);
						BW.Write (filter.Length);
						BW.Write (filter);
					}
					return MS.ToArray ();
				}
			} 
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("eacfc35a-cc3d-4d2c-a27f-669dd41894ee");
			}
		}
		#endregion
	}
}

