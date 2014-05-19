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
using BD2.Frontend.Table.Model;

namespace BD2.Frontend.Table
{
	public class FrontendInstance : BD2.Frontend.Table.Model.FrontendInstance
	{
		SortedDictionary<Guid, BaseDataObjectTypeIdAttribute> typeDescriptors = new SortedDictionary<Guid, BaseDataObjectTypeIdAttribute> ();
		SortedDictionary<byte[], Row> rows = new SortedDictionary<byte[], Row> (new BD2.Common.ByteSequenceComparer ());
		SortedDictionary<byte[], Table> tables = new SortedDictionary<byte[], Table> (new BD2.Common.ByteSequenceComparer ());
		SortedDictionary<byte[], Relation> relations = new SortedDictionary<byte[], Relation> (new BD2.Common.ByteSequenceComparer ());
		SortedDictionary<byte[], Column> columns = new SortedDictionary<byte[], Column> (new BD2.Common.ByteSequenceComparer ());
		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (new BD2.Common.ByteSequenceComparer ());
		ValueSerializerBase valueSerializer;
		SortedDictionary<byte[], BaseDataObject> volatileData = new SortedDictionary<byte[], BaseDataObject> (new BD2.Common.ByteSequenceComparer ());

		public override ValueSerializerBase ValueSerializer {
			get {
				return valueSerializer;
			}
		}

		Frontend frontend;

		public FrontendInstance (Snapshot snapshot, Frontend frontend, ValueSerializerBase valueSerializer):
			base(snapshot, frontend)
		{
			if (valueSerializer == null)
				throw new ArgumentNullException ("valueSerializer");
			this.valueSerializer = valueSerializer;
		}
		#region implemented abstract members of FrontendInstanceBase
		protected override void OnCreateObjects (byte[] bytes)
		{
			System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes);
			byte[] buf = new byte[16];
			while (MS.Position< MS.Length) {
				MS.Read (buf, 0, 16);
				Guid objectTypeID = new Guid (buf);
				BaseDataObjectTypeIdAttribute typeDescriptor = typeDescriptors [objectTypeID];

			}
		}

		protected override IEnumerable<BaseDataObject> GetVolatileObjects ()
		{
			throw new NotImplementedException ();
		}

		protected override IEnumerable<BaseDataObject> GetObjectWithID (byte[] objectID)
		{
			throw new NotImplementedException ();
		}

		protected override void PurgeObject (BaseDataObject baseDataObject)
		{
			if (baseDataObject is Row) {
				
			} else if (baseDataObject is Column) {

			} else if (baseDataObject is BD2.Frontend.Table.Model.ColumnSet) {

			} else if (baseDataObject is Table) {
			
			} else if (baseDataObject is Relation) {
			
			}
		}
		#endregion
		//to avoid duplicates
		public override BD2.Frontend.Table.Model.Column GetColumn (string name, Type type, bool allowNull, long length)
		{
			Column nc = new Column (this, null, name, type, allowNull, length);
			byte[] hash = nc.GetPersistentUniqueObjectID ();
			if (columns.ContainsKey (hash)) 
				return columns [hash];
			volatileData.Add (hash, nc);
			columns.Add (hash, nc);
			return nc;
		}

		public override ColumnSet GetColumnSet (Model.Column[] columns)
		{
			if (columns == null)
				throw new ArgumentNullException ("columns");
			ColumnSet cs = new ColumnSet (this, null, columns);
			byte[] hash = cs.GetPersistentUniqueObjectID ();
			if (columnSets.ContainsKey (hash))
				return columnSets [hash];
			volatileData.Add (hash, cs);
			columnSets.Add (hash, cs);
			return cs;
		}

		public BD2.Frontend.Table.Row CreateRow (BD2.Frontend.Table.Model.Table table, BD2.Frontend.Table.Model.ColumnSet columnSet, object[] objects)
		{
			Row r = new Row (this, null, table, columnSet, objects);
			volatileData.Add (r.GetPersistentUniqueObjectID (), r);
			return r;
		}

		public void Flush ()
		{
			Snapshot.PutObjects (GetVolatileObjects ());
		}
		#region implemented abstract members of FrontendInstance
		public override BD2.Frontend.Table.Model.Table GetTable (string name)
		{
			throw new NotImplementedException ();
		}

		public override IEnumerable<BD2.Frontend.Table.Model.Row> GetRows (BD2.Frontend.Table.Model.Table table)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
