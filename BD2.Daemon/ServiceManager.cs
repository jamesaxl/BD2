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
using System.Collections.Generic;

namespace BD2.Daemon
{
	public class ServiceManager
	{
		ObjectBus objectBus;
		SortedDictionary<Guid, ServiceAnnouncement> localServices = new SortedDictionary<Guid, ServiceAnnouncement> ();
		SortedDictionary<ServiceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, Action, ServiceAgent>> localServiceAgents = new SortedDictionary<ServiceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, Action, ServiceAgent>> ();
		SortedSet<ServiceAnnouncement> remoteServices = new SortedSet<ServiceAnnouncement> ();
		SortedDictionary<Guid, Tuple<ServiceRequest, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent>> requests = new SortedDictionary<Guid, Tuple<ServiceRequest, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent>> ();
		SortedDictionary<Guid, ServiceResponse> pendingResponses = new SortedDictionary<Guid, ServiceResponse> ();
		SortedDictionary<Guid, ServiceAgent> sessionAgents = new  SortedDictionary<Guid, ServiceAgent> ();

		public ServiceManager (ObjectBus objectBus)
		{
			if (objectBus == null)
				throw new ArgumentNullException ("objectBus");
			this.objectBus = objectBus;
			objectBus.RegisterType (typeof(ServiceAnnouncement), ServiceAnnouncementReceived);
			objectBus.RegisterType (typeof(ServiceResponse), ServiceResponseReceived);
			objectBus.RegisterType (typeof(ServiceRequest), ServiceRequestReceived);
			objectBus.RegisterType (typeof(ServiceDestroy), ServiceDestroyReceived);
			objectBus.Start ();
		}

		void ServiceAnnouncementReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceAnnouncement))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceAnnouncement).FullName));
			ServiceAnnouncement serviceAnnouncement = (ServiceAnnouncement)message;
			lock (remoteServices)
				remoteServices.Add (serviceAnnouncement);
		}

		void ServiceResponseReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceResponse))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceResponse).FullName));
			ServiceResponse serviceResponse = (ServiceResponse)message;
			Tuple<ServiceRequest, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent> requestTuple = requests [serviceResponse.RequestID];
			lock (pendingResponses)
				pendingResponses.Add (serviceResponse.RequestID, serviceResponse);
			requestTuple.Item2.Set ();
			requestTuple.Item3.WaitOne ();
		}

		void ServiceRequestReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceRequest))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceRequest).FullName));
			ServiceRequest serviceRequest = (ServiceRequest)message;
			Func<ServiceAgentMode , ObjectBusSession, Action, ServiceAgent> agentFunc;
			lock (localServices)
				lock (localServiceAgents)
					agentFunc = localServiceAgents [localServices [serviceRequest.ServiceID]];
			Guid responseID = Guid.NewGuid ();
			ObjectBusSession session = objectBus.CreateSession (responseID, SessionDisconnected);
			ServiceResponse response = new ServiceResponse (responseID, serviceRequest.ID, ServiceResponseStatus.Accepted);
			ServiceAgent agent = agentFunc.Invoke (ServiceAgentMode.Server, session, objectBus.Flush);
			lock (sessionAgents)
				sessionAgents.Add (session.SessionID, agent);
			objectBus.SendMessage (response);
		}

		void SessionDisconnected (ObjectBusSession session)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			lock (sessionAgents)
				sessionAgents [session.SessionID].SessionDisconnected ();
		}

		void ServiceDestroyReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceDestroy)) {
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceDestroy).FullName));
			}
			ServiceDestroy serviceDestroy = (ServiceDestroy)message;
			lock (sessionAgents)
				sessionAgents [serviceDestroy.SessionID].DestroyRequestReceived ();
			objectBus.DestroySession (serviceDestroy);

		}

		public SortedSet<ServiceAnnouncement> EnumerateRemoteServices ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			lock (remoteServices)
				return new SortedSet<ServiceAnnouncement> (remoteServices);
		}

		public void AnnounceService (ServiceAnnouncement serviceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, Action, ServiceAgent> func)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			if (serviceAnnouncement == null)
				throw new ArgumentNullException ("serviceAnnouncement");
			if (func == null)
				throw new ArgumentNullException ("func");
			lock (localServices)
				lock (localServiceAgents) {
					localServices.Add (serviceAnnouncement.ID, serviceAnnouncement);
					localServiceAgents.Add (serviceAnnouncement, func);
				}
			objectBus.SendMessage (serviceAnnouncement);
		}

		public ServiceAgent RequestService (ServiceAnnouncement remoteServiceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, Action, ServiceAgent> func)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			lock (remoteServices)
				if (!remoteServices.Contains (remoteServiceAnnouncement))
					throw new InvalidOperationException ("The provided remoteServiceAnnouncement is not valid.");
			ServiceRequest request = new ServiceRequest (Guid.NewGuid (), remoteServiceAnnouncement.ID);
			System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent (false);
			System.Threading.ManualResetEvent mre_done = new System.Threading.ManualResetEvent (false);

			lock (requests)
				requests.Add (request.ID, new Tuple<ServiceRequest, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent> (request, mre, mre_done));
			objectBus.SendMessage (request);
			mre.WaitOne ();
			//todo: add exception handling here
			ServiceResponse response = pendingResponses [request.ID];
			lock (pendingResponses)
				pendingResponses.Remove (response.RequestID);
			ServiceAgent agent = func (ServiceAgentMode.Client, objectBus.CreateSession (response.ID, SessionDisconnected), objectBus.Flush);
			sessionAgents.Add (response.ID, agent);
			mre_done.Set ();
			return agent;
		}
	}
}