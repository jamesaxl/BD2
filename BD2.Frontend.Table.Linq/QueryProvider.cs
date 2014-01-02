//
//  QueryProvider.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
namespace BD2.Frontend.Table.Linq
{
	public class QueryProvider : System.Linq.IQueryProvider
	{
		public QueryProvider ()
		{
		}
		#region IQueryProvider implementation
		IQueryable IQueryProvider.CreateQuery (System.Linq.Expressions.Expression expression)
		{
			throw new System.NotImplementedException ();
		}

		object IQueryProvider.Execute (System.Linq.Expressions.Expression expression)
		{
			throw new System.NotImplementedException ();
		}

		public IQueryable<TElement> CreateQuery<TElement> (System.Linq.Expressions.Expression expression)
		{
			throw new System.NotImplementedException ();
		}

		public TResult Execute<TResult> (System.Linq.Expressions.Expression expression)
		{
			throw new System.NotImplementedException ();
		}
		#endregion

	}
}

