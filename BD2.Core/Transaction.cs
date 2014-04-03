using System;
using System.Collections.Generic;
using BD2.Core;

namespace BD2.Core
{
	public sealed class Transaction
	{
		string comment;
		object transaction_lock = new object ();
		TransactionStatus status = TransactionStatus.Pending;
		List<Tuple<BaseDataObject, long>> objects = new List<Tuple<BaseDataObject, long>> (32);

		internal string Comment {
			get {
				return comment;
			}
		}

		internal List<Tuple<BaseDataObject, long>> Objects {
			get {
				return objects;
			}
		}

		public Transaction (string comment)
		{
			this.comment = comment;
		}

		internal void AddObject (BaseDataObject baseDataObject, long proxyComboId = 0)
		{
			lock (transaction_lock) {
				if (status != TransactionStatus.Pending) {
					throw new InvalidOperationException ("Cannot alter a commited/rolled back transaction.");
				}
				System.Threading.Monitor.Enter (objects);
			}
			objects.Add (new Tuple<BaseDataObject, long> (baseDataObject, proxyComboId));
			System.Threading.Monitor.Exit (objects);
		}

		TransactionStatus Status {
			get {
				return status;
			}
		}
	}
}
