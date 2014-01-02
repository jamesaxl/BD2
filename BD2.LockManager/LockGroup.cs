//
//  LockState.cs
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

namespace BD2.LockManager
{
	public sealed class LockGroup
	{
		List<Tuple<Guid, Action<LockGroup, Guid>>> requestList;
		SortedSet<LockState> lockStates = new SortedSet<LockState> ();

		public Guid Request (Action<LockGroup, Guid> Callback)
		{
			Guid ID = Guid.NewGuid ();
			lock (requestList)
				requestList.Add (new Tuple<Guid, Action<LockGroup, Guid>> (ID, Callback));
			return ID;
		}

		private void ExecuteRequestList ()
		{
			lock (requestList) {
				foreach (var Tuple in requestList) {
					Tuple.Item2 (this, Tuple.Item1);
				}
			}
		}

		internal float CompareWith (LockGroup lockGroup)
		{
			float sum = 0;
			SortedSet<LockState> Except = this.LockStates;
			Except.ExceptWith (lockGroup.LockStates);
			foreach (LockState ls in Except) {
				sum += ls.Size;
			}
			return sum;
		}

		internal void NotifyAcquired ()
		{
			lock (lockStates) {
				int Offset = 0;
				LockState[] LockedList = new LockState[lockStates.Count];//I have to do this, if I don't and I run out of memory bad things are gonna happen. 
				try {
					foreach (LockState LockState in lockStates) {
						if (System.Threading.Monitor.TryEnter (LockState)) {
							LockedList [Offset++] = LockState;
							if (LockState.Status == LockStatus.Unlocked) {
								throw new SystemException ("LockManager has messed up.");
							}
						} else {
							throw new SystemException ("LockManager has messed up.");
						}
					}
					ExecuteRequestList ();
				} catch (Exception ex) {
					if (Offset != LockedList.Length) {
						throw new Exception (string.Format ("Something wicked happened while acquiring locks.Got {0} out of {1} locks.", Offset, LockedList.Length), ex);
					}
				} finally {
					for (int n = 0; n != Offset; n++) {
						System.Threading.Monitor.Exit (LockedList [n]);
					}
				}
			}
		}

		public LockStatus Status {
			get {
				foreach (LockState ls in LockStates) {
					if (ls.Status == LockStatus.Unlocked)
						return LockStatus.Unlocked;
				}
				return LockStatus.Locked;
			}
		}

		public SortedSet<LockState> LockStates {
			get {
				lock (lockStates)
					return new SortedSet<LockState> (lockStates);
			}
		}

		public LockGroup (IEnumerable<LockState> LockStates)
		{
			if (LockStates == null)
				throw new ArgumentNullException ("LockStates");
			lockStates = new SortedSet<LockState> (lockStates);
		}
	}
}
