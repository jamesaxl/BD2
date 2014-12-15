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
	public class FrontendInstance : FrontendInstanceBase
	{
		SortedDictionary<byte[], Table> tables = new SortedDictionary<byte[], Table> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], Column> columns = new SortedDictionary<byte[], Column> (ByteSequenceComparer.Shared);
		SortedDictionary<byte[], ColumnSet> columnSets = new SortedDictionary<byte[], ColumnSet> (ByteSequenceComparer.Shared);
		SortedDictionary<ColumnSet, SortedDictionary<ColumnSet, ColumnSetConverter>> cscs = new SortedDictionary<ColumnSet, SortedDictionary<ColumnSet, ColumnSetConverter>> ();

		protected override BaseMetaObject GetObjectWithID (byte[] objectID)
		{
			//todo:use TryGetValue
			if (columns.ContainsKey (objectID))
				return columns [objectID];
			if (columnSets.ContainsKey (objectID))
				return columnSets [objectID];
			if (tables.ContainsKey (objectID))
				return tables [objectID];
			return null;
		}

		public void AddColumnSetConverter (ColumnSetConverter csc)
		{
			foreach (ColumnSet ocs in csc.OutColumnSets) {
				if (!cscs.ContainsKey (ocs)) {
					cscs.Add (ocs, new SortedDictionary<ColumnSet, ColumnSetConverter> ());
				}
				SortedDictionary<ColumnSet, ColumnSetConverter> sources = cscs [ocs];
				foreach (ColumnSet ics in csc.InColumnSets) {
					sources.Add (ics, csc);
				}
			}
		}

		public ColumnSetConverter GetColumnSetConverter (ColumnSet columnSet, ColumnSet outputColumnSet)
		{
			if (cscs.ContainsKey (outputColumnSet)) {
				SortedDictionary<ColumnSet, ColumnSetConverter> sources = cscs [outputColumnSet];
				if (sources.ContainsKey (columnSet)) {
					return sources [columnSet];
				} else
					throw new NotSupportedException ("Conversion from source ColumnSet is not supported.");			
			} else
				throw new NotSupportedException ("Conversion to destination ColumnSet is not supported.");
		}

		public object[] ConvertColumnSet (object[] input, ColumnSet inputColumnSet, ColumnSet outputColumnSet)
		{
			if (inputColumnSet == outputColumnSet)
				return input;
			return GetColumnSetConverter (inputColumnSet, outputColumnSet).Convert (input, inputColumnSet, outputColumnSet);
		}

		ValueSerializerBase valueSerializer;
		//SortedDictionary<byte[], BaseDataObject> volatileData = new SortedDictionary<byte[], BaseDataObject> (BD2.Common.ByteSequenceComparer.Shared);
		public ValueSerializerBase ValueSerializer {
			get {
				return valueSerializer;
			}
		}

		public FrontendInstance (Frontend frontend, ValueSerializerBase valueSerializer) :
			base (frontend)
		{
			if (valueSerializer == null)
				throw new ArgumentNullException ("valueSerializer");
			this.valueSerializer = valueSerializer;
		}

		#region implemented abstract members of FrontendInstance

		void InsertObject (BaseDataObjectVersion baseDataObjectVersion)
		{
			throw new NotImplementedException ();
		}

		protected override void OnCreateObjects (byte[] chunkID, byte[] bytes)
		{
			using (System.IO.MemoryStream MS = new System.IO.MemoryStream (bytes)) {
				using (System.IO.BinaryReader BR = new System.IO.BinaryReader (MS)) {
					int objectCount = BR.ReadInt32 ();
					Console.WriteLine ("Deserializing {0} objects", objectCount);
					for (int n = 0; n != objectCount; n++) {
						int objectLengeth = BR.ReadInt32 ();
						//Console.WriteLine ("Length:{0}", objectLengeth);
						Guid objectTypeID = new Guid (BR.ReadBytes (16));
						BaseDataObjectTypeIdAttribute typeDescriptor = BaseDataObjectTypeIdAttribute.GetAttribFor (objectTypeID);
						InsertObject (typeDescriptor.Deserialize (this, chunkID, BR.ReadBytes (objectLengeth - 16)));
					}
				}
			}
		}


		#endregion

		public  ColumnSet GetColumnSetByID (byte[] id)
		{
			if (!columnSets.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return columnSets [id];
		}

		public  Table GetTableByID (byte[] id)
		{
			if (!tables.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return tables [id];
		}

		public  Column GetColumnByID (byte[] id)
		{
			if (!columns.ContainsKey (id))
				System.Diagnostics.Debugger.Break ();
			return columns [id];
		}

		public IEnumerable<Table> GetTables ()
		{
			return new SortedSet<Table> (tables.Values);
		}

		//to avoid duplicates
		public Column GetColumn (string name, Type type, bool allowNull, long length)
		{
			Column nc = new Column (this, null, null, name, type, allowNull, length);
			byte[] hash = nc.ObjectID;
			if (columns.ContainsKey (hash))
				return columns [hash];
			columns.Add (hash, nc);
			return nc;
		}

		public ColumnSet GetColumnSet (Column[] columns)
		{
			if (columns == null)
				throw new ArgumentNullException ("columns");
			ColumnSet cs = new ColumnSet (this, null, null, columns);
			byte[] hash = cs.ObjectID;
			if (columnSets.ContainsKey (hash)) {
				return columnSets [hash];
			}
			columnSets.Add (hash, cs);
			return cs;
		}

		public Table GetTable (string name)
		{
			Table temp = new Table (this, null, null, name);
			if (tables.ContainsKey (temp.ObjectID)) {
				return tables [temp.ObjectID];
			}
			tables.Add (temp.ObjectID, temp);
			//perTableRows.Add (temp, new SortedDictionary<byte[], Row> (ByteSequenceComparer.Shared));
			return temp;
		}


		public IEnumerable<ColumnSet> GetColumnSets ()
		{
			return new SortedSet<ColumnSet> (columnSets.Values);
		}
	}
}
