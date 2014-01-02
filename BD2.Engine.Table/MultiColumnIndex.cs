using System;
using System.Collections.Generic;
using BSO;
using BD2.Common;
using BD2.Common.Model;
namespace BD2.Frontend.Table
{
	
	public sealed class MultiColumnIndex : IndexBase
	{
		IndexColumnBase[] indexColumns;
		public override IEnumerator<IndexColumnBase> GetIndexColumns ()
		{
			foreach (IndexColumnBase indexColumn in indexColumns)
				yield return indexColumn;
		}
		public MultiColumnIndex(bool Unique, IndexColumnBase[] IndexColumns)
		{
			if (IndexColumns == null)
				throw new ArgumentNullException ("IndexColumn");
			foreach (IndexColumnBase IC in IndexColumns) {
				if (IC == null)
					throw new ArgumentException ("IndexColumn must not contain null enteries.","IndexColumn");
			}
			unique = Unique;
			indexColumns = ((IndexColumnBase[])IndexColumns.Clone());
			objectID = Guid.NewGuid ();
		}
		bool unique;
		public override bool Unique {
			get {
				return unique;
			}
		}

		#region implemented abstract members of Serializable

		public override void Serialize (System.IO.BinaryWriter Stream)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region implemented abstract members of BaseDataObject

		public override BaseDataObject Drop ()
		{
			return new BD2.ObjectDrop(Guid.NewGuid(), this);
		}
		Guid objectID;
		public override Guid ObjectID {
			get {
				return objectID;
			}
		}

		#endregion
	}
	
}
