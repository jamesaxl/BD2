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

namespace BD2.Chunk.Daemon.Common
{
	[ObjectBusMessageTypeID("fee3227b-92b1-42e3-8f7b-a544e8e81d50")]
	[ObjectBusMessageDeserializer(typeof(TopLevelChunksRequestMessage), "Deserialize")]
	public class TopLevelChunksResponseMessage : ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		Guid chunks;

		public Guid Chunks {
			get {
				return chunks;
			}
		}

		Exception exception;

		public Exception Exception {
			get {
				return exception;
			}
		}

		public TopLevelChunksResponseMessage (Guid requestID, Guid chunks, Exception exception)
		{
			this.requestID = requestID;
			this.chunks = chunks;
			this.exception = exception;
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS  = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (requestID.ToByteArray ());
					BW.Write (chunks.ToByteArray ());
				}
				if (exception == null) {
					MS.WriteByte (0);
				} else {
					MS.WriteByte (1);
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BF = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
					BF.Serialize (MS, exception);
				}
				return MS.ToArray ();
			}
		}

		public static ObjectBusMessage Deserialize (byte[] buffer)
		{
			Guid requestID;
			Guid chunks;
			Exception exception;

			using (System.IO.MemoryStream MS  = new System.IO.MemoryStream (buffer,false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					requestID = new Guid (BR.ReadBytes (16));
					chunks = new Guid (BR.ReadBytes (16));
				}
				if (MS.ReadByte () == 1) {
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BF = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
					object deserializedObject = BF.Deserialize (MS);
					if (deserializedObject is Exception) {
						exception = (Exception)deserializedObject;
					} else {
						throw new Exception ("buffer contains an object of invalid type, expected System.Exception.");
					}
				} else
					exception = null;
			}
			return new TopLevelChunksResponseMessage (requestID, chunks, exception);
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("fee3227b-92b1-42e3-8f7b-a544e8e81d50");
			}
		}
		#endregion
	}
}

