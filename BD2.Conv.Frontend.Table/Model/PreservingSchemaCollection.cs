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
using System.Collections.Generic;

namespace BD2.Conv.Frontend.Table
{
	public sealed class PreservingSchemaCollection
	{
		SortedDictionary<string, Table> tablesByName = new SortedDictionary<string, Table> ();
		SortedDictionary<Guid, Table> tablesByID = new SortedDictionary<Guid, Table> ();
		SortedDictionary<Table, SortedDictionary<string, Column>> perTableColumnsByName = new SortedDictionary<Table, SortedDictionary<string, Column>> ();
		SortedDictionary<Table, SortedDictionary<Guid, Column>> perTableColumnsByID = new SortedDictionary<Table, SortedDictionary<Guid, Column>> ();

		public void AddTable (Table t)
		{
			tablesByID.Add (t.ID, t);
			tablesByName.Add (t.Name, t);
			perTableColumnsByID.Add (t, new SortedDictionary<Guid, Column> ());
			perTableColumnsByName.Add (t, new SortedDictionary<string, Column> ());
		}

		public Table GetTableByID (Guid id)
		{
			return tablesByID [id];
		}

		public Table GetTableByName (string name)
		{
			return tablesByName [name];
		}

		public void AddColumn (Table table, Column column)
		{
			perTableColumnsByID [table].Add (column.ID, column);
			perTableColumnsByName [table].Add (column.Name, column);

		}

		public Column GetColumnByID (Table table, Guid id)
		{
			return perTableColumnsByID [table] [id];
		}

		public Column GetColumnByName (Table table, string name)
		{
//			try {
			return perTableColumnsByName [table] [name];
//			} catch {
//				 (table.Name);
//				 (name);
//
//				 ();
//				foreach (var ct in perTableColumnsByName[table]) {
//					 (ct.Value.Name);
//				}
//				throw;
//			}
		}

		public void AddColumnSet (ColumnSet columnSet)
		{
		}

		public ColumnSet GetColumnSetByID (Guid id)
		{
			return null;
		}

		public void AddForeignKeyRelation (ForeignKeyRelation foreignKeyRelation)
		{
		}

		public ForeignKeyRelation GetForeignKeyRelationByID (Guid id)
		{
			return null;
		}

		ForeignKeyRelation GetForeignKeyRelationByName (string name)
		{
			return null;
		}
	}
}

