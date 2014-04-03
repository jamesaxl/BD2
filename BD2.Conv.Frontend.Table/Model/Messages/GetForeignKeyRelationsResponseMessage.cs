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

namespace BD2.Conv.Frontend.Table
{
	public class GetForeignKeyRelationsResponseMessage : BD2.Daemon.ObjectBusMessage
	{
		Guid requestID;

		public Guid RequestID {
			get {
				return requestID;
			}
		}

		ForeignKeyRelation[] foreignKeyRelations;

		public ForeignKeyRelation[] ForeignKeyRelations {
			get {
				return (ForeignKeyRelation[])foreignKeyRelations.Clone ();
			}
		}

		Exception exception;

		public GetForeignKeyRelationsResponseMessage (Guid requestID, ForeignKeyRelation[] foreignKeyRelations, Exception exception)
		{
			if (foreignKeyRelations == null)
				throw new ArgumentNullException ("foreignKeyRelations");
			this.requestID = requestID;
			this.foreignKeyRelations = foreignKeyRelations;
			this.exception = exception;
		}

		public static ObjectBusMessage Deserialize (byte[] bytes)
		{
			Guid requestID;
			ForeignKeyRelation[] foreignKeyRelations;
			Exception exception;
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes, false)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					requestID = new Guid (BR.ReadBytes (16));
					foreignKeyRelations = new ForeignKeyRelation[BR.ReadInt32 ()];
					for (int n = 0; n != foreignKeyRelations.Length; n++) {
						foreignKeyRelations [n] = ForeignKeyRelation.Deserialize (BR.ReadBytes (BR.ReadInt32 ()));
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
					return new GetForeignKeyRelationsResponseMessage (requestID, foreignKeyRelations, exception);
				}
			}
		}
		#region implemented abstract members of ObjectBusMessage
		public override byte[] GetMessageBody ()
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (requestID.ToByteArray ());
					BW.Write (foreignKeyRelations.Length);
					for (int n =0; n != foreignKeyRelations.Length; n++) {
						byte[] bytes = foreignKeyRelations [n].Serialize ();
						BW.Write (bytes.Length);
						BW.Write (bytes);
					}
					if (exception == null) {
						MS.WriteByte (0);
					} else {
						MS.WriteByte (1);
						System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BF = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
						BF.Serialize (MS, exception);
					}
					return MS.GetBuffer ();
				}
			}
		}

		public override Guid TypeID {
			get {
				return Guid.Parse ("716b9fbf-fab5-4c0e-abae-307f9895c6cc");
			}
		}
		#endregion
	}
}

