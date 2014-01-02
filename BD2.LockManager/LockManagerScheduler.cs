//
//  LockManagerScheduler.cs
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
	public abstract class LockManagerScheduler
	{
		Action<LockGroup> timedOut;
		Action<LockGroup> perform;

		protected Action<LockGroup> Perform {
			get {
				return perform;
			}
		}

		internal abstract void Schedule (LockGroup lockGroup);

		internal abstract void PerformNext ();
	}

	sealed class LockManagerScheduler_Deadline : LockManagerScheduler
	{

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

		comparer_CFQ comparer = null;
		LockGroup Current = null;
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