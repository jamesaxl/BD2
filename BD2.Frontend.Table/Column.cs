/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
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
using BD2.Core;
using BD2.Daemon;

namespace BD2.Frontend.Table
{
	[ObjectBusMessageTypeID("85997e6a-60d3-4dfb-ae49-6bd7a0de4b60")]
	[ObjectBusMessageDeserializer(typeof(Column), "Deserialize")]
	public class Column : Model.Column
	{
		internal Column (FrontendInstanceBase frontendInstanceBase, byte[] chunkID, string name, long typeID, bool allowNull, long length)
			:base(frontendInstanceBase, chunkID, name, typeID, allowNull, length)
		{
		}
		#region implemented abstract members of Serializable
		public override void Serialize (System.IO.Stream stream)
		{
			base.Serialize (stream);
		}
		#endregion
		#region implemented abstract members of BaseDataObject
		public override Guid ObjectType {
			get {
				return Guid.Parse ("85997e6a-60d3-4dfb-ae49-6bd7a0de4b60");
			}
		}
		#endregion
		internal static byte[] Hash (string name, long typeID, bool allowNull, long length)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream ()) {
				using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (MS)) {
					BW.Write (name);
					BW.Write (typeID);
					BW.Write (allowNull);
					BW.Write (length);
				}
				System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create ();
				return sha.ComputeHash (MS.ToArray ());
			}
		}
	}
}
