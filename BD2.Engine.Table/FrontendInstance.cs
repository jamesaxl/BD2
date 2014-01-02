//
//  FrontendInstance.cs
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
using System.Collections.Generic;
using BD2.Common;

namespace BD2.Frontend.Table
{
	public class FrontendInstance : FrontendInstanceBase
	{
		SortedSet<Table> tables;
		SortedSet<Relation> relations;

		public FrontendInstance ()
		{
		}

		#region IFrontendInstance implementation

		public bool Dynamic {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public Transaction CreateTransaction ()
		{
			throw new System.NotImplementedException ();
		}

		public bool ApplyTransaction ()
		{
			throw new System.NotImplementedException ();
		}

		#region implemented abstract members of FrontendInstanceBase

		protected override void Commit (Transaction transaction)
		{
			throw new NotImplementedException ();
		}

		protected override Transaction CreateTransactionInstance ()
		{
			throw new NotImplementedException ();
		}

		public override BD2.Common.Frontend Frontend {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		public Snapshot Snapshot {
			get {
				throw new System.NotImplementedException ();
			}
		}

		#endregion

	}
}

