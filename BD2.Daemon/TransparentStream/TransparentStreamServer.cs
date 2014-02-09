using System;

namespace BD2.Daemon
{
	sealed class TransparentStreamServer
	{
		int maximumSatisfiableUnit;
		Guid streamID;
		ServiceAgent agent;
		System.IO.Stream storageBackend;
		System.Collections.Concurrent.ConcurrentDictionary<long, int> pendingRequests;
		bool canRead;
		bool canWrite;

		public TransparentStreamServer (ServiceAgent agent, System.IO.Stream storageBackend)
			:this(agent,storageBackend, short.MaxValue)
		{

		}

		public TransparentStreamServer (ServiceAgent agent, System.IO.Stream storageBackend, int maximumSatisfiableUnit)
		{
			if (agent == null)
				throw new ArgumentNullException ("agent");
			if (storageBackend == null)
				throw new ArgumentNullException ("storageBackend");
			this.maximumSatisfiableUnit = maximumSatisfiableUnit;
			this.streamID = Guid.NewGuid ();
			this.agent = agent;
			this.storageBackend = storageBackend;
			pendingRequests = new System.Collections.Concurrent.ConcurrentDictionary<long, int> ();
		}

		public ServiceAgent Agent {
			get {
				return agent;
			}
		}

		public bool CanWrite {
			get {
				return canWrite;
			}
		}

		public bool CanRead {
			get {
				return canRead;
			}
		}

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		public int MaximumSatisfiableUnit {
			get {
				return maximumSatisfiableUnit;
			}
		}
	}
}
