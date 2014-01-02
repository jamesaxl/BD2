using System;
using System.Collections.Generic;
using BD2.Common;


namespace BD2.Frontend.Table.Model
{	
	public abstract class Column : BaseDataObject
	{
		public abstract ColumnSet ColumnSet { get; internal set; }
		public abstract string Name { get; }
		public abstract long Length { get; }
		public abstract bool AllowNull { get; }
		public abstract bool PrioratizedOffset { get; }
		int HashCode;
		public override int GetHashCode ()
		{
			//atomic
			if (HashCode == 0) {
				HashCode = Name.GetHashCode() ^ ((int)(Length >> 32))^((int)Length) ^ (AllowNull ? 0x7C3B9473 : 0);
			}
			return base.GetHashCode ();
		}
		public  bool TypeEquals (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			Column OtherColumn = obj as Column;
			if (OtherColumn == null)
				throw new ArgumentException ("obj must be of type Column.", "obj");
			return  (OtherColumn.AllowNull == this.AllowNull) && (OtherColumn.Length == this.Length);
		}
		#region implemented abstract members of Serializable
		public override void Serialize (System.IO.BinaryWriter Stream)
		{
			base.Serialize(Stream);//Just to raise an error.do not raise it here. more stack trace helps debugging
		}
		#endregion
	}
}
