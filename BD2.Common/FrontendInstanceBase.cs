//
//  IFrontendInstance.cs
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
	public abstract class FrontendInstanceBase : IDisposable
	{
		public abstract Snapshot Snapshot { get; }

		public abstract Frontend Frontend { get; }

		public abstract Transaction CreateTransaction ();

		protected abstract void Commit (Transaction transaction);

		public abstract bool Dynamic { get; }

		SortedSet<Transaction> transactions;

		public IEnumerable<Transaction> GetTransactions ()
		{
			return (IEnumerable<Transaction>)transactions;
		}

		public Transaction BeginTransaction ()
		{
			Transaction trans = CreateTransactionInstance ();
			//TODO:set optional attributes here
			return trans;
		}

		protected abstract Transaction CreateTransactionInstance ();

		internal bool CommitTransaction (ChunkData transaction)
		{
			if (transaction == null)
				throw new ArgumentNullException ("transaction");
			if (transaction.FrontendInstance != this)
				throw new InvalidOperationException ("Transaction does not belong to this snapshot.");
			byte[] ChunkBytes = transaction.ConvertToChunk (1);
			return Frontend.CommitTransaction (transaction);
		}

		internal void RollbackTransaction (Transaction transaction)
		{
			if (transaction == null)
				throw new ArgumentNullException ("transaction");
			if (transaction.FrontendInstance != this)
				throw new InvalidOperationException ("Transaction does not belong to this snapshot/frontend.");
			if (!transactions.Contains (transaction))
				throw new InvalidOperationException ("Transaction has already been rolled back.");
			transactions.Remove (transaction);
		}
	}
}
