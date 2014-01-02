//
//  LockManager.cs
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
	public sealed class LockManager
	{
		SortedSet<LockRequest> requests = new  SortedSet<LockRequest> ();

		internal void EnqueueRequest (LockRequest lockRequest)
		{
			if (lockRequest == null)
				throw new ArgumentNullException ("lockRequest");
			lock (requests) {
				requests.Add (lockRequest);
			}
		}

		/// <summary>
		/// Dequeue the specified lock.
		/// </summary>
		internal void Dequeue (LockRequest lockRequest)
		{
			if (lockRequest == null)
				throw new ArgumentNullException ("lockRequest");
			LockGroup testLockGroup;
			lock (requests) {
				if (requests.Contains (lockRequest)) {
					requests.Remove (lockRequest);
				} else {
					throw new AccessViolationException ("The specified lock doesn't match the lock used to request the lock");
				}
			}
		}

		SortedSet<LockState> pendingLocks;

		private LockState NextPendingLockState {
			get {
				lock (lock_access) {
					//return first object in pendingLocks
					throw new NotImplementedException ();
				}
			}
		}

		SortedSet<LockState> staleLocks;

		public SortedSet<LockState> StaleLocks {
			get {
				lock (locks) {
					return new SortedSet <LockState> (staleLocks);
				}
			}
		}

		SortedDictionary<LockState, Action<LockState>> locks;

		public SortedDictionary<LockState, Action<LockState>> Locks {
			get {
				lock (locks) {
					return new SortedDictionary <LockState, Action<LockState>> (locks);
				}
			}
		}

		SortedSet<LockGroup> lockGroups;

		public SortedSet<LockGroup> LockGroups {
			get {
				lock (lockGroups) {
					return new SortedSet<LockGroup> (lockGroups);
				}
			}
		}

		object lock_access;

		public object Lock_access {
			get {
				return lock_access;
			}
		}

		public LockManager ()
		{
			pendingLocks = new  SortedSet<LockState> ();
			locks = new SortedDictionary<LockState, Action<LockState>> ();
			lock_access = new object ();
		}

		internal LockState CreateLock (LockType type, LockableBase[] BaseDataObjectStateTrackers, int TimeoutMilliSeconds, Action<LockState> StateChanged)
		{
			//TODO: aqcuire lock here or provide option to do so later and make a call-back for it.
			try {
				LockState ls = new LockState (this, type, BaseDataObjectStateTrackers, NotifyStateChanged, ReportStale, TimeSpan.FromMilliseconds (TimeoutMilliSeconds));
				lock (lock_access) {
					pendingLocks.Add (ls);
				}
				return ls;
			} catch {
				return null;
			} finally {
			}
		}

		private void NotifyStateChanged (LockState LockState)
		{
			Action<LockState> act = locks [LockState];
			switch (LockState.Status) {
			case LockStatus.Locked:

				break;
			case LockStatus.Unlocked:
				break;
			}
			act (LockState);
		}

		private void ReportStale (LockState lockState)
		{
			lock (lock_access)
				staleLocks.Add (lockState);
		}

		public LockStatus StatusOf (LockState lockState)
		{
			lock (lock_access) {
				if (staleLocks.Contains (lockState)) {
					return LockStatus.Locked;//not sure
				}
				return LockStatus.Unlocked;
			}
		}

		public void ProcessRequest (LockRequest LR)
		{
			throw new NotImplementedException ();
		}
	}
}