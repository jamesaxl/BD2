//
//  BaseDataObjectStateTracker.cs
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

namespace BD2.Common
{
	public abstract class BaseDataObjectStateTracker
	{
		BaseDataObject bdo;
		FrontendInstanceBase fib;
		BaseDataObjectState currentState;

		protected BaseDataObjectState CurrentState {
			get {
				return currentState;
			}
		}

		internal BaseDataObject Bdo {
			get {
				return bdo;
			}
		}

		internal FrontendInstanceBase Fib {
			get {
				return fib;
			}
		}

		private void Assert ()
		{
			LockState ls = null;
			try {
				//FIXME: remove this stupid assertion stub.
				ls = bdo.Frontend.LockManager.CreateLock (this);
				LockGroup LG = new LockGroup (new LockState[]{ ls });
				LockRequest LR = new LockRequest (TimeSpan.FromSeconds (15), () => {
				});
				bdo.Frontend.LockManager.ProcessRequest (LR);
				if (bdo.Frontend.LockManager)
				if (bdo.GetTrackerFor (fib) != this) {
					throw new SystemException ("BaseDataObjectStateTracker assertien failed, memory management error detected.");
				}
			} catch {
				throw new SystemException ("BaseDataObjectStateTracker assertien failed, seriously unexpected memory management error detected.");
			} finally {
				if (ls != null)
					ls.Release ();
			}
		}

		public bool Valid {
			get;
			set;
		}
	}
}
