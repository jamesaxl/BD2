using System;
using System.Collections.Generic;

namespace BD2.Frontend.Table.Model
{
	
	public abstract class UserDefinedColumn : Column
	{
		public abstract IEnumerator<Type> ParameterTypes { get; }
		public abstract IComparable GetValue (Row Row);
		public abstract void SetValue (Row Row, IComparable Value);
		public abstract IComparable GetValue (Row Row, object[] Parameters);
		public abstract void SetValue (Row Row, IComparable Value, object[] Parameters);
	}
	
}
