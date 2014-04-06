using System;

namespace BD2.Frontend.Table.Model
{
	public abstract class ValueSet
	{
		Row row;

		public Row Row {
			get {
				return row;
			}
		}

		protected ValueSet (Row row)
		{
			if (row == null)
				throw new ArgumentNullException ("row");
			this.row = row;

		}

		public abstract object GetValue (Column column);
	}
}

