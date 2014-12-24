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
	[ObjectBusMessageTypeIDAttribute ("415bd4c1-274c-42d9-8201-975253753245")]
	[ObjectBusMessageDeserializerAttribute (typeof(TransparentStreamReadResponseMessage), "Deserialize")]
	sealed class TransparentStreamReadResponseMessage : TransparentStreamMessageBase
	{
		Guid streamID;

		public override Guid StreamID {
			get {
				return streamID;
			}
		}

		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		byte[] data;

		public byte[] Data {
			get {
				return data;
			}
		}

		Exception exception;

		public Exception Exception {
			get {
				return exception;
			}
		}

		public TransparentStreamReadResponseMessage (Guid streamID, Guid requestID, byte[] data, Exception exception)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			this.streamID = streamID;
			this.requestID = requestID;
			this.data = data;
			this.exception = exception;
		}

		public static TransparentStreamReadResponseMessage Deserialize (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			Guid streamID;
			Guid requestID;
			byte[] data;
			Exception exception;
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					streamID = new Guid (BR.ReadBytes (16));
					requestID = new Guid (BR.ReadBytes (16));
					data = BR.ReadBytes (BR.ReadInt32 ());
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
			}
			return new TransparentStreamReadResponseMessage (streamID, requestID, data, exception);
		}

		#region implemented abstract members of ObjectBusMessage

		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (streamID.ToByteArray ());
					BW.Write (requestID.ToByteArray ());
					BW.Write (data.Length);
					BW.Write (data);
					if (exception == null) {
						MS.WriteByte (0);
					} else {
						MS.WriteByte (1);
						System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BF = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
						BF.Serialize (MS, exception);
					}
				}
				return MS.ToArray ();
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("415bd4c1-274c-42d9-8201-975253753245");
			}
		}

		#endregion
	}
}
