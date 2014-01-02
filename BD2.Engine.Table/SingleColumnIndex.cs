using System;
using System.Collections.Generic;
using BSO;
namespace BD2
{
	public sealed class SingleColumnIndex : IndexBase
	{
		IndexColumnBase indexColumn;
		public override IEnumerator<IndexColumnBase> GetIndexColumns ()
		{
			yield return indexColumn;
		}
		public SingleColumnIndex(bool Unique, IndexColumnBase IndexColumn)
		{
			if (IndexColumn == null)
				throw new ArgumentNullException ("IndexColumn");
			indexColumn = IndexColumn;
			unique = Unique;
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

		public override Guid ObjectID {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
	
}
