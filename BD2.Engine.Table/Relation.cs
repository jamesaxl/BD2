using System;
using System.Collections.Generic;
using BSO;
using BD2.Frontend.Table.Model;
using BD2.Common;


namespace BD2.Frontend.Table
{
	public sealed class Relation : Model.Relation
	{
		public class Comparer : IComparer<Relation>
		{
			public int Compare (Relation x, Relation y)
			{
				int hashCompare = x.GetHashCode ().CompareTo (y.GetHashCode());
				if (hashCompare != 0)
					return hashCompare;
				throw new NotImplementedException();
			}
		}
		IndexBase[] parentColumns;
		public override IEnumerator<IndexBase> ParentColumns
		{
			get
			{
				foreach (IndexBase index in parentColumns)
					yield return index;
			}
		}
		Column[] childColumns;
		public override IEnumerator<Column> ChildColumns
		{
			get
			{
				foreach (Column column in childColumns)
					yield return column;
			}
		}
		public override void Serialize (System.IO.BinaryWriter Stream)
		{
			throw new NotImplementedException ();
		}
		public override BaseDataObject Drop ()
		{
			return new ObjectDrop (this);
		}

		Guid objectID;
		public override Guid ObjectID {
			get {
				return objectID;
			}
		} 
		public Relation(IndexBase[] ParentColumns, Column[] ChildColumns, Guid ObjectID) {
			parentColumns = ParentColumns;
			childColumns = ChildColumns;
			objectID = ObjectID;
		}
	}
	
}
