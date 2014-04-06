/*
 * Copyright (c) 2013-2014 Behrooz Amoozad
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the bd2 nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Behrooz Amoozad BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
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
		//stub, to avoid a 'warning as error'
		private SortedSet<LockState> ProperSubsets {
			get {
				return properSubsets;
			}
		}

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
					lock (lockManager.LockAccess) {
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
