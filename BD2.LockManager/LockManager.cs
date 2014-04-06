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
				lock (lockAccess) {
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

		SortedSet<LockGroup> lockGroups = new SortedSet<LockGroup> ();

		public SortedSet<LockGroup> LockGroups {
			get {
				lock (lockGroups) {
					return new SortedSet<LockGroup> (lockGroups);
				}
			}
		}

		object lockAccess;

		public object LockAccess {
			get {
				return lockAccess;
			}
		}

		public LockManager ()
		{
			pendingLocks = new  SortedSet<LockState> ();
			locks = new SortedDictionary<LockState, Action<LockState>> ();
			lockAccess = new object ();
		}

		public LockState CreateLock (LockType type, LockableBase[] baseDataObjectStateTrackers, int timeoutMilliSeconds, Action<LockState> stateChanged)
		{
			//TODO: aqcuire lock here or provide option to do so later and make a call-back for it.
			try {
				LockState ls = new LockState (this, type, baseDataObjectStateTrackers, NotifyStateChanged, ReportStale, TimeSpan.FromMilliseconds (timeoutMilliSeconds));
				lock (lockAccess) {
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
			lock (lockAccess)
				staleLocks.Add (lockState);
		}

		public LockStatus StatusOf (LockState lockState)
		{
			lock (lockAccess) {
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