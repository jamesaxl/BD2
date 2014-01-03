//
//  BaseDataObjectState.cs
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
	public abstract class BaseDataObjectState
	{
		BaseDataObjectStateTracker tracker;

		public BaseDataObjectStateTracker Tracker {
			get {
				return tracker;
			}
		}

		public abstract bool CanApplyToParent ();

		public abstract void ApplyToParent ();

		protected abstract void OnDrop ();

		public abstract IEnumerable<BaseDataObjectState> GetDependencies ();

		public abstract IEnumerable<BaseDataObjectState> GetLiveReferers ();

		BaseDataObjectState parent;

		public BaseDataObjectState Parent {
			get {
				if (parent == null)
					throw new InvalidOperationException ("BaseDataObjectState is top-level thus has no parent.");
				return parent;
			}
		}

		public bool IsTopLevel {
			get { 
				return parent == null; 
			}
		}

		Stack<Tuple<Transaction, BaseDataObjectState>> subStates;
	}
	
}
