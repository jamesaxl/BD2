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
using BD2.Core;

namespace BD2.Frontend.Table.Model
{
	public sealed class RowDrop : BaseDataObjectVersion
	{
		Row row;

		public Row Row {
			get {
				return row;
			}
		}

		public RowDrop (byte[] chunkID,
		                BaseDataObject baseDataObject,
		                byte[][] previousVersionChunkIDs,
		                BaseDataObjectVersion[] previousVersions,
		                Row  row)
			: base (chunkID, baseDataObject, previousVersionChunkIDs, previousVersions)
		{
			if (row == null)
				throw new ArgumentNullException ("row");
			this.row = row;
		}

		#region implemented abstract members of Serializable

		public static RowDrop Deserialize (FrontendInstanceBase fib, byte[] chunkID, byte[] buffer)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (buffer)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					return new RowDrop (fib, chunkID, ((BD2.Frontend.Table.Model.FrontendInstance)fib).GetRowByID (BR.ReadBytes (32)));
				}
			}
		}

		public override void Serialize (System.IO.Stream stream)
		{
			using (System.IO.BinaryWriter BW = new System.IO.BinaryWriter (stream)) {
				BW.Write (row.ObjectID);
			}
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override Guid ObjectType {
			get {
				return Guid.Parse ("1ede8774-cdd5-4d88-bce2-daa9af54aa51");
			}
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override System.Collections.Generic.IEnumerable<BaseDataObject> GetDependenies ()
		{
			yield return row;
		}

		#endregion
	}
}

