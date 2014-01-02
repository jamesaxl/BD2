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
	public sealed class LockState
	{
		float size = float.NaN;
		TimeSpan defaultTimeout;
		Guid staticID = Guid.NewGuid ();
		SortedSet<LockState> properSubsets = new SortedSet<LockState> ();
		LockManager lockManager;
		/// <summary>
		/// object to Exchange when trying to change states
		/// </summary>
		object lock_status;
		/// <summary>
		/// Specifies whether this locks allows read or write operations to be performed on the collection.
		/// </summary>
		LockType lockType;
		SortedSet<LockableBase> lockables;
		Action<LockState> stateChanged;

		/// <summary>
		/// Approximate size of the lock, used for scheduling with addition of timeout and elapsed time.base-2 log.
		/// </summary>
		/// <value>The size.</value>
		public float Size {
			get {
				if (float.IsNaN (size))
					size = (float)Math.Log (lockables.Count, 2);
				return size;
			}
		}

		public Action<LockState> StateChanged {
			get {
				return stateChanged;
			}
		}

		internal TimeSpan DefaultTimeout {
			get {
				return defaultTimeout;
			}
		}

		public Guid StaticID {
			get {
				return new Guid (staticID.ToByteArray ());
			}
		}

		bool SetAsAquired ()
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Specifies whether this lock is currently acquired or not
		/// </summary>
		/// <value>The status.</value>
		public LockStatus Status {
			get {
				return lockManager.StatusOf (this);
			}
		}

		/// <summary>
		/// Specifies whether this locks allows read or write operations to be performed on the collection.
		/// </summary>
		public LockType LockType {
			get {
				return lockType;
			}
		}

		public bool ContainsObjectStateTracker (LockableBase BaseDataObjectStateTracker)
		{
			if (BaseDataObjectStateTracker == null)
				throw new ArgumentNullException ("BDOST");
			return  BaseDataObjectStateTrackers.Contains (BaseDataObjectStateTracker);
		}

		private bool CanCoexistWithType (LockType Type)
		{
			return ((this.lockType | Type) & LockType.Write) != LockType.Write;
		}

		internal bool CanCoexistWith (LockState LockState)
		{
			if (CanCoexistWithType (LockState.lockType))
				return true;//fine anyway, we're both probably reading
			//bit if we have anything in common, We've got to wait
			return !LockState.lockables.Overlaps (this.lockables);

		}

		public SortedSet<LockableBase> BaseDataObjectStateTrackers {
			get {
				return new SortedSet<LockableBase> (lockables);
			}
		}

		Action<LockState> reportStale;

		internal LockState (LockManager LockManager, LockType LockType, IEnumerable<LockableBase> BaseDataObjectStateTrackers, Action<LockState> StateChanged, Action<LockState> ReportStale, TimeSpan Timeout)
		{
			if (LockManager == null)
				throw new ArgumentNullException ("LockManager");
			if (BaseDataObjectStateTrackers == null)
				throw new ArgumentNullException ("BaseDataObjects");
			if (StateChanged == null)
				throw new ArgumentNullException ("StateChanged");
			if (ReportStale == null)
				throw new ArgumentNullException ("ReportStale");
			lockManager = LockManager;
			lock_status = new object ();
			reportStale = ReportStale;
			lockType = LockType;
			lockables = new SortedSet<LockableBase> (BaseDataObjectStateTrackers);
			stateChanged = StateChanged;
			defaultTimeout = Timeout;
			if (Timeout == TimeSpan.Zero) {
				Timeout = TimeSpan.MaxValue;
			}
		}

		private bool VerifyObjects ()
		{
			foreach (LockableBase BDOST in BaseDataObjectStateTrackers) {
				if (!BDOST.Valid) {
					return false;
				}
			}
			return true;
		}

		internal void TryAcquire (LockGroup Group)
		{
			lock (lock_status) {
				if (Status == LockStatus.Unlocked) {
					lock (lockManager.Lock_access) {
						foreach (var LockState in lockManager.Locks) {
							if (LockState.Key != this)
							if (!LockState.Key.CanCoexistWith (this)) {
								return;
							}
						}
						if (VerifyObjects ()) {
							stateChanged (this);						
						}
					}
				}
			}
		}

		internal void Release ()
		{
			lock (lock_status) {
				if (Status != LockStatus.Locked)//this condition is better off inside the lock, because it can avoid exceptions being cought by outher threads as it may happen sometims 
					throw new Exception ("Lock is already released, possibly by a request in parallel with this one.");
				reportStale (this);
			}
		}
	}
}
