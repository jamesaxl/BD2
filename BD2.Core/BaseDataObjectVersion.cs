// /*
//  * Copyright (c) 2014 Behrooz Amoozad
//  * All rights reserved.
//  *
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the bd2 nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  *
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  * */
using System;
using System.Collections.Generic;

namespace BD2.Core
{
	public abstract class BaseDataObjectVersion : Serializable, IComparable<BaseDataObjectVersion>
	{
		byte[] id;
		byte[] chunkID;
		BaseDataObject baseDataObject;
		IDictionary<byte[], BaseDataObjectVersion> previousVersions;

		public byte[] ChunkID {
			get {
				if (chunkID == null)
					return null;
				return (byte[])chunkID.Clone ();
			}
		}

		public byte[] ID {
			get {
				return id;
			}
		}

		public BaseDataObject BaseDataObject {
			get {
				return baseDataObject;
			}
		}

		public IDictionary<byte[], BaseDataObjectVersion> PreviousVersions {
			get {
				return previousVersions;
			}
		}

		public abstract Guid ObjectType { get; }

		public virtual IEnumerable<BaseDataObjectVersion> GetDependenies ()
		{
			yield break;
		}

		protected BaseDataObjectVersion (byte[] id,
		                                 byte[] chunkID,
		                                 BaseDataObject baseDataObject)
		{
			if (baseDataObject == null)
				throw new ArgumentNullException ("baseDataObject");
			this.id = id;
			this.chunkID = chunkID;
			this.baseDataObject = baseDataObject;
			this.previousVersions = new Dictionary<byte[], BaseDataObjectVersion> (baseDataObject.Versions);
		}

		#region IComparable implementation

		public int CompareTo (BaseDataObjectVersion other)
		{
			throw new NotImplementedException ();
		}


		public bool IsVolatile {
			get {
				return chunkID == null;
			}
		}

 
		internal void SetChunkID (byte[] newChunkID)
		{
			if (newChunkID == null)
				throw new ArgumentNullException ("newChunkID");
			if (chunkID != null)
				throw new InvalidOperationException ();
			chunkID = newChunkID;
		}

		#endregion
	}
}

