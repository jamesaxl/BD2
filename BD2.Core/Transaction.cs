using System;
using System.Collections.Generic;
using System.IO;
using BSO;
using BD2.Core;

namespace BD2
{
	public sealed class Transaction
	{
		Guid author;
		string comment;
		object transaction_lock = new object ();
		TransactionStatus status = TransactionStatus.Pending;
		FrontendInstanceBase frontendInstance;
		SortedDictionary<Guid, RawProxy.RawProxyv1> rawProxyDefs = new SortedDictionary<Guid, BD2.RawProxy.RawProxyv1> ();
		List<Guid[]> rawProxyyCombinations = new List<Guid[]> ();
		System.Collections.Generic.List<Tuple<BaseDataObject, Reference<long>>> objects = new List<Tuple<BaseDataObject, Reference<long>>> (32);
		SortedSet<Transaction> childeren;

		public SortedSet<Transaction> Childeren {
			get {
				lock (childeren) {
					return new SortedSet<Transaction> (childeren);
				}
			}
		}

		Transaction parent;

		public Transaction Parent {
			get {
				if (parent == null)
					throw new InvalidOperationException ("transaction is top-level thus has no parent.");
				return parent;
			}
		}

		public bool IsTopLevel {
			get { 
				return parent == null; 
			}
		}

		internal Guid Author {
			get {
				return author;
			}
		}

		internal string Comment {
			get {
				return comment;
			}
		}

		internal System.Collections.Generic.List<Tuple<BaseDataObject, Reference<long>>> Objects {
			get {
				return objects;
			}
		}

		public Guid declareRawProxy (RawProxy.RawProxyv1 Item)
		{
			Guid ID = Guid.NewGuid ();
			rawProxyDefs.Add (ID, Item);
			return ID;
		}

		/// <summary>
		/// the first one is the default
		/// </summary>
		/// <returns>Index of raw proxy combo added.</returns>
		/// <param name="Items">Items.</param>
		public int DeclareRawProxyCombo (Guid[] Items)
		{
			lock (transaction_lock) {
				rawProxyyCombinations.Add (Items);
				return rawProxyyCombinations.Count - 1;
			}
		}

		public FrontendInstanceBase FrontendInstance {
			get {
				return frontendInstance;
			}
		}

		public Transaction (FrontendInstanceBase FrontendInstance)
		{
			if (FrontendInstance == null)
				throw new ArgumentNullException ("FrontendInstance");
			if (!frontendInstance.Dynamic)
				throw new InvalidOperationException ("Static snapshots cannot have transactions.");
			frontendInstance = FrontendInstance;
		}

		internal void AddObject (BaseDataObject BaseDataObject, long ProxyComboID = 0)
		{
			lock (transaction_lock) {
				if (status != TransactionStatus.Pending) {
					throw new InvalidOperationException ("Cannot alter a commited/rolled back transaction.");
				}
				System.Threading.Monitor.Enter (objects);
			}
			objects.Add (new Tuple<BaseDataObject, Reference<long>> (BaseDataObject, ProxyComboID));
			System.Threading.Monitor.Exit (objects);
		}

		TransactionStatus Status {
			get {
				return status;
			}
		}

		public void Commit ()
		{
			lock (transaction_lock) {
				if (status != TransactionStatus.Pending) {
					throw new InvalidOperationException ("Cannot commit an already commited/rolled back transaction.");
				}
				status = TransactionStatus.Commiting;
				if (frontendInstance.CommitTransaction (this)) {
					status = TransactionStatus.Commited;
				} else {
					status = TransactionStatus.RolledBack;
				}
			}
		}

		public void RollBack ()
		{
			lock (transaction_lock) {
				if (status == TransactionStatus.Commited) {
					throw new InvalidOperationException ("Transaction has been queued for commited.It may be long gone.\nThere is almost nothing you can do about it now except pulling the plug.");
				}
				frontendInstance.RollbackTransaction (this);
			}
		}
	}
}
