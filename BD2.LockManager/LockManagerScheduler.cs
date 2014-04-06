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
	public abstract class LockManagerScheduler
	{
		Action<LockGroup> timedOut;

		protected Action<LockGroup> TimedOut {
			get {
				return timedOut;
			}
		}

		Action<LockGroup> perform;

		protected Action<LockGroup> Perform {
			get {
				return perform;
			}
		}

		protected LockManagerScheduler (Action<LockGroup> timedOut, Action<LockGroup> perform)
		{
			if (timedOut == null)
				throw new ArgumentNullException ("timedOut");
			if (perform == null)
				throw new ArgumentNullException ("perform");
			this.timedOut = timedOut;
			this.perform = perform;
		}

		internal abstract void Schedule (LockGroup lockGroup);

		internal abstract void PerformNext ();
	}

	sealed class LockManagerScheduler_Deadline : LockManagerScheduler
	{
		public LockManagerScheduler_Deadline (Action<LockGroup> timedOut, Action<LockGroup> perform)
			:base(timedOut, perform)
		{
		}
		#region implemented abstract members of LockManagerScheduler
		internal override void Schedule (LockGroup lockGroup)
		{
			throw new NotImplementedException ();
		}

		internal override void PerformNext ()
		{
			throw new NotImplementedException ();
		}
		#endregion
	}

	sealed class LockManagerScheduler_CFQ : LockManagerScheduler
	{
		public LockManagerScheduler_CFQ (Action<LockGroup> timedOut, Action<LockGroup> perform)
			:base(timedOut, perform)
		{
		}

		class comparer_CFQ : IComparer<LockGroup>
		{
			LockGroup current;

			internal void SetCurrent (LockGroup Current)
			{
				current = Current;
			}
			#region IComparer implementation
			int IComparer<LockGroup>.Compare (LockGroup x, LockGroup y)
			{
				return y.CompareWith (current).CompareTo (y.CompareWith (current));
			}
			#endregion
		}

		List<LockGroup> LGs = new List<LockGroup> (128);
		#region implemented abstract members of LockManagerScheduler
		internal override void Schedule (LockGroup lockGroup)
		{
			lock (LGs) {
				LGs.Add (lockGroup);
			}
		}

		internal override void PerformNext ()
		{

		}
		#endregion
	}
}