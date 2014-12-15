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
using BD2.Frontend.Table;

namespace BD2.Frontend.Table
{
	[BaseDataObjectTypeIdAttribute ("3be06a16-6727-4639-b702-060b522af660", typeof(Table), "Deserialize")]
	public class Table : BaseMetaObject
	{
		string name;

		public string Name { get { return name; } }

		public Table (FrontendInstanceBase frontendInstanceBase, byte[] objectID, byte[] chunkID, string name)
			: base (frontendInstanceBase, objectID, chunkID)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.name = name;
		}

		#region implemented abstract members of Serializable

		public static BaseMetaObject Deserialize (FrontendInstanceBase fib, byte[] chunkID, byte[] buffer)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new Table (fib, BR.ReadBytes (32), chunkID, BR.ReadString ());
				}
			}
		}

		public override void Serialize (System.IO.Stream stream, EncryptedStorageManager encryptedStorageManager)
		{
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (name);
			}
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override IEnumerable<BaseMetaObject> GetDependenies ()
		{
			yield break;
		}

		public override Guid ObjectType {
			get {
				return Guid.Parse ("3be06a16-6727-4639-b702-060b522af660");
			}
		}

		#endregion
	}
}
