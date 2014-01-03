//
//  FrontendCollection.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 behrooz
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

namespace BD2.Core
{
	/// <summary>
	/// Yet another layer to make sure we are in control.
	/// </summary>
	public class FrontendCollection
	{
		SortedSet<Frontend> frontends = new SortedSet<Frontend> ();
		Database database;

		public FrontendCollection (Database database)
		{
			if (database == null)
				throw new ArgumentNullException ("database");
			this.database = database;

		}

		public void AddFrontend (Frontend Item)
		{
			if (Item == null)
				throw new ArgumentNullException ("Item");
			lock (frontends) {
				frontends.Add (Item);
			}
		}

		public SortedSet<Frontend> GetFrontends ()
		{
			SortedSet<Frontend> ret;
			lock (frontends) {
				ret = new SortedSet<Frontend> (frontends);
			}
			return ret;
		}

		internal void CommitTransaction (Transaction transaction)
		{
			database.CommitTransaction (transaction);
		}

		internal void HandleTransaction (Transaction transaction)
		{
			foreach (Frontend Frontend in GetFrontends ()) {
				Frontend.CommitTransaction (transaction);
			}
		}
	}
}
