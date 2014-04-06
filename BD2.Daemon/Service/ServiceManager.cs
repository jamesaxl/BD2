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

namespace BD2.Daemon
{
	public class ServiceManager
	{
		ObjectBus objectBus;
		SortedDictionary<Guid, ServiceAnnounceMessage> localServices = new SortedDictionary<Guid, ServiceAnnounceMessage> ();
		SortedDictionary<ServiceAnnounceMessage, Func<ServiceAgentMode , ObjectBusSession, Action, Byte[], ServiceAgent>> localServiceAgents = new SortedDictionary<ServiceAnnounceMessage, Func<ServiceAgentMode , ObjectBusSession, Action, Byte[], ServiceAgent>> ();
		SortedSet<ServiceAnnounceMessage> remoteServices = new SortedSet<ServiceAnnounceMessage> ();
		SortedDictionary<Guid, Tuple<ServiceRequestMessage, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent>> requests = new SortedDictionary<Guid, Tuple<ServiceRequestMessage, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent>> ();
		SortedDictionary<Guid, ServiceResponseMessage> pendingResponses = new SortedDictionary<Guid, ServiceResponseMessage> ();
		SortedDictionary<Guid, ServiceAgent> sessionAgents = new  SortedDictionary<Guid, ServiceAgent> ();

		public ServiceManager (ObjectBus objectBus)
		{
			if (objectBus == null)
				throw new ArgumentNullException ("objectBus");
			this.objectBus = objectBus;
			objectBus.RegisterType (typeof(ServiceAnnounceMessage), ServiceAnnouncementReceived);
			objectBus.RegisterType (typeof(ServiceResponseMessage), ServiceResponseReceived);
			objectBus.RegisterType (typeof(ServiceRequestMessage), ServiceRequestReceived);
			objectBus.RegisterType (typeof(ServiceDestroyMessage), ServiceDestroyReceived);
			objectBus.Start ();
		}

		void ServiceAnnouncementReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceAnnounceMessage))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceAnnounceMessage).FullName));
			ServiceAnnounceMessage serviceAnnouncement = (ServiceAnnounceMessage)message;
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
			if (!(message is ServiceResponseMessage))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceResponseMessage).FullName));
			ServiceResponseMessage serviceResponse = (ServiceResponseMessage)message;
			Tuple<ServiceRequestMessage, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent> requestTuple = requests [serviceResponse.RequestID];
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
			if (!(message is ServiceRequestMessage))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceRequestMessage).FullName));
			ServiceRequestMessage serviceRequest = (ServiceRequestMessage)message;
			Func<ServiceAgentMode , ObjectBusSession, Action, Byte[], ServiceAgent> agentFunc;
			lock (localServices)
				lock (localServiceAgents)
					agentFunc = localServiceAgents [localServices [serviceRequest.ServiceID]];
			Guid responseID = Guid.NewGuid ();
			ObjectBusSession session = objectBus.CreateSession (responseID, SessionDisconnected);
			ServiceResponseMessage response = new ServiceResponseMessage (responseID, serviceRequest.ID, ServiceResponseStatus.Accepted);
			ServiceAgent agent = agentFunc.Invoke (ServiceAgentMode.Server, session, objectBus.Flush, serviceRequest.Parameters);
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
				sessionAgents [session.SessionID].CallSessionDisconnected ();
		}

		void ServiceDestroyReceived (ObjectBusMessage message)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceDestroyMessage)) {
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceDestroyMessage).FullName));
			}
			ServiceDestroyMessage serviceDestroy = (ServiceDestroyMessage)message;
			lock (sessionAgents)
				sessionAgents [serviceDestroy.SessionID].CallDestroyRequestReceived ();
			objectBus.DestroySession (serviceDestroy);

		}

		public SortedSet<ServiceAnnounceMessage> EnumerateRemoteServices ()
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif
			lock (remoteServices)
				return new SortedSet<ServiceAnnounceMessage> (remoteServices);
		}

		public void AnnounceService (ServiceAnnounceMessage serviceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, Action, byte[], ServiceAgent> func)
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

		public ServiceAgent RequestService (ServiceAnnounceMessage remoteServiceAnnouncement, byte[] parameters, Func<ServiceAgentMode , ObjectBusSession, Action, Byte[], ServiceAgent> func, Byte[] localAgentParameters)
		{
			#if TRACE
			Console.WriteLine (new System.Diagnostics.StackTrace (true).GetFrame (0));
			#endif

			lock (remoteServices)
				if (!remoteServices.Contains (remoteServiceAnnouncement))
					throw new InvalidOperationException ("The provided remoteServiceAnnouncement is not valid.");
			ServiceRequestMessage request = new ServiceRequestMessage (Guid.NewGuid (), remoteServiceAnnouncement.ID, parameters);
			System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent (false);
			System.Threading.ManualResetEvent mre_done = new System.Threading.ManualResetEvent (false);

			lock (requests)
				requests.Add (request.ID, new Tuple<ServiceRequestMessage, System.Threading.ManualResetEvent, System.Threading.ManualResetEvent> (request, mre, mre_done));
			objectBus.SendMessage (request);
			mre.WaitOne ();
			//todo: add exception handling here
			ServiceResponseMessage response = pendingResponses [request.ID];
			lock (pendingResponses)
				pendingResponses.Remove (response.RequestID);
			ServiceAgent agent = func (ServiceAgentMode.Client, objectBus.CreateSession (response.ID, SessionDisconnected), objectBus.Flush, localAgentParameters);
			sessionAgents.Add (response.ID, agent);
			mre_done.Set ();
			return agent;
		}
	}
}