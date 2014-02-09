/*
 * Copyright (c) 2014 Behrooz Amoozad
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
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */
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
