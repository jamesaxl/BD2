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
using BD2.Daemon;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace BD2.Chunk.Daemon
{
	public class ChunkAgent : ServiceAgent
	{
		ConcurrentQueue<RequestTopLevelChunkDeltaMessage> pendingRemoteRequests = new ConcurrentQueue<RequestTopLevelChunkDeltaMessage> ();
		ConcurrentQueue<RequestTopLevelChunkDeltaMessage> pendingLocalRequests = new ConcurrentQueue<RequestTopLevelChunkDeltaMessage> ();
		int requestQueueLengthThresholdMin = 64;
		int requestQueueLengthThresholdMax = 512;
		SortedSet<byte[]> requests = new SortedSet<byte[]> ();
		SortedSet<byte[]> pendingRequests = new SortedSet<byte[]> ();
		ChunkRepository repository;

		public ChunkAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, ChunkRepository repository)
			:base(serviceAgentMode, objectBusSession, flush)
		{
			if (repository == null)
				throw new ArgumentNullException ("repository");
			this.repository = repository;
			objectBusSession.RegisterType (typeof(RequestTopLevelChunkDeltaMessage), RequestTopLevelChunkDeltaMessageReceived);
			objectBusSession.RegisterType (typeof(PushChunkMessage), PushChunkMessageReceived);

		}

		public static ServiceAgent CreateAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, ChunkRepository repository)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (repository == null)
				throw new ArgumentNullException ("repository");
			return new ChunkAgent (serviceAgentMode, objectBusSession, flush, repository);
		}

		void RequestTopLevelChunkDeltaMessageReceived (ObjectBusMessage  message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			RequestTopLevelChunkDeltaMessage requestTopLevelChunkDeltaMessage = (RequestTopLevelChunkDeltaMessage)message;
			pendingRemoteRequests.Enqueue (requestTopLevelChunkDeltaMessage);
		}

		void PushChunkMessageReceived (ObjectBusMessage obj)
		{
			//todo:add to repository
			//todo:remove item from requests, if it exists at all
			//todo:check tx queue and send another batch if it's empty
		}

		void SendRequests ()
		{
		}

		void AddRequestFor (IEnumerable<byte[]> chunkID)
		{
			//add item(s) to pendingrequests
			//todo:sendRequests() 
		}

		void ComputeDelta (IEnumerable<IRangedFilter> remoteFilters, out SortedSet<byte[]> weNeed, out SortedSet<byte[]> theyNeed)
		{
			if (remoteFilters == null)
				throw new ArgumentNullException ("remoteFilters");
			weNeed = new SortedSet<byte[]> ();
			SortedSet<byte[]> localChunks = new SortedSet<byte[]> (repository.EnumerateTopLevels ());
			foreach (IRangedFilter IRF in remoteFilters) {
				SortedSet<byte[]> section = localChunks.GetViewBetween (IRF.FirstChunk, IRF.LastChunk);
				switch (IRF.FilterTypeName) {
				case "List":
					//add items to weNeed and theyNeed
					RangedListFilter RLF = (RangedListFilter)IRF;
					theyNeed = new SortedSet<byte[]> (section);
					theyNeed.ExceptWith (RLF.Items);
					weNeed = new SortedSet<byte[]> (RLF.Items);//I know, there will be a better workaround in future.
					weNeed.ExceptWith (section);
					break;
			
				default:
					throw new InvalidOperationException ("BD2.Chunk.Daemon doesn't support anything further than simple lists right now. cope with it.");
					break;
				}
			}
			//empty datasets on remote? hmmm
			weNeed = new SortedSet<byte[]> ();
			theyNeed = new SortedSet<byte[]> ();
		}

		void DoInitialSync ()
		{
			SortedSet<IRangedFilter> filters = CreateFilters (new [] { repository.EnumerateTopLevels () });
			RequestTopLevelChunkDeltaMessage RTLCDM = new RequestTopLevelChunkDeltaMessage (Guid.NewGuid (), filters);
			ObjectBusSession.SendMessage (RTLCDM);
		}

		void KeepSync ()
		{

		}

		void WaitForSync ()
		{

		}

		static int GetSplitBits (IList<byte> chunkID, int depth, int bits)
		{
			int r = 0;
			for (int n = 0; n != bits; n++) {
				if ((chunkID [(depth + n) / 8] & (1 << (depth + n))) != 0)
					r |= 1 << n;
			}
			return r;
		}

		static byte GetState (int count)
		{
			//TODO:All states and RangedFilter classes along with all the unclean code they have with them should be 
			//put in external libraries and used as plugins.
			if (count == 0) {
				return state_empty;
			} else if (count < 16) {
				return state_few;
			} else if (count < 32) {
				return state_normal;
			} else if (count < 128) {
				return state_many;
			} else {
				return state_full;
			}
		}
		// skip
		const int state_empty = 0;
		//merge with neighbors
		const int state_few = 1;
		//list //try neighbors
		const int state_normal = 2;
		//bf 
		const int state_many = 3;
		//split and retry
		const int state_full = 4;

		SortedSet<IRangedFilter> CreateFilters (IEnumerable<IEnumerable<byte[]>> topLevelLists, int depth = 0)
		{
			SortedSet<IRangedFilter> filters = new SortedSet<IRangedFilter> ();
			int splitbits = 8;
			int bitexp = 1 << splitbits;
			SortedSet<byte[]>[] buckets = new SortedSet<byte[]>[bitexp];
			foreach (IEnumerable<byte[]> TL in topLevelLists)
				foreach (byte[] chunk in TL) {
					int bitid = GetSplitBits (chunk, depth, splitbits);
					buckets [bitid].Add (chunk);
				}
			int total = 0;
			List<SortedSet<byte[]>> toplevellistsection = new List<SortedSet<byte[]>> ();

			for (int n = 0; n != bitexp; n++) {
				total += buckets [n].Count;
				byte state = GetState (total);
				toplevellistsection.Add (buckets [n]);
				if (state == 4) {
					filters.UnionWith (CreateFilters (toplevellistsection, depth + splitbits));
					foreach (SortedSet<byte[]> bucket in toplevellistsection)
						bucket.Clear ();
					toplevellistsection.Clear ();
					total = 0;
				} else if (state == 3) {
					RangedListFilter RBF = new RangedListFilter (toplevellistsection);
					filters.Add (RBF);
					foreach (SortedSet<byte[]> bucket in toplevellistsection)
						bucket.Clear ();
					toplevellistsection.Clear ();
					total = 0;
				} else if (state == 2) {
					if (n + 1 < bitexp) {
						if (GetState (total + buckets [n + 1].Count) == 2)
							continue;
					}
					RangedListFilter RLF = new RangedListFilter (toplevellistsection);
					filters.Add (RLF);
					foreach (SortedSet<byte[]> bucket in toplevellistsection)
						bucket.Clear ();
					toplevellistsection.Clear ();
					total = 0;
				}
			}
			//the last few items left
			{
				RangedListFilter RLF = new RangedListFilter (toplevellistsection);
				filters.Add (RLF);
				foreach (SortedSet<byte[]> bucket in toplevellistsection)
					bucket.Clear ();
				toplevellistsection.Clear ();
				total = 0;				
			}
			return filters;
		}
		#region implemented abstract members of ServiceAgent
		protected override void Run ()
		{
			//we may need another thread to answer the other side, network cannot wait for our disk i/o.
			//a bunch of method calls like this
			DoInitialSync ();
			Flush ();
			WaitForSync ();
			KeepSync ();
		}

		public override void DestroyRequestReceived ()
		{

		}

		public override void SessionDisconnected ()
		{

		}
		#endregion
	}
}

