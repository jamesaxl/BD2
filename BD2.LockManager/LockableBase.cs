//
//  ILockable.cs
//
//  Author:
//       Behrooz Amoozad <behrooz0az@gmail.com>
//
//  Copyright (c) 2013 Behrooz Amoozad
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

namespace BD2.LockManager
{
	public abstract class LockableBase
	{

		volatile int locked = 0;
		System.Collections.Generic.SortedSet<LockState> managers = new System.Collections.Generic.SortedSet<LockState> ();

		void AddLockStateTracker (LockState state)
		{
			lock (managers) {
				managers.Add (state);
			}
		}

		void RemoveLockManager (LockState state)
		{
			lock (managers) {
				managers.Remove (state);
			}
		}

		public System.Collections.Generic.IEnumerable<LockState> GetHoldingLock ()
		{
			//not freaking worth the micro-optimizatio
			lock (managers) {
				foreach (LockState ls in managers)
					if (ls.Status == LockStatus.Locked)
						yield return ls;
			}
			yield break;
		}

		public abstract bool Valid { get; }
	}
}
// REQUIREMENTS:
// NO EXTRA MEMORY CUNSIMPTION OTHER THAN ONE REFERENCE MADE BY LOCKSTATE
// NO VIRTUAL CALL OR AT LEAST NO EXTRA REDIRECTION
// NA NEED TO WRITE ADDITIONAL CODE AT IMPLEMNTOR CLASSES THAN ALREADY IS NEEDED
//
//
//
//
//
