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
