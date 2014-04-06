using System;
using System.Linq;

namespace BD2.Frontend.Table.Linq
{
	public class OrderedQueryable : System.Linq.IOrderedQueryable
	{
		public OrderedQueryable ()
		{
		}
		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			throw new System.NotImplementedException ();
		}
		#endregion
		#region IQueryable implementation
		Type IQueryable.ElementType {
			get {
				throw new System.NotImplementedException ();
			}
		}

		System.Linq.Expressions.Expression IQueryable.Expression {
			get {
				throw new System.NotImplementedException ();
			}
		}

		System.Linq.IQueryProvider IQueryable.Provider {
			get {
				throw new System.NotImplementedException ();
			}
		}
		#endregion
	}
}

