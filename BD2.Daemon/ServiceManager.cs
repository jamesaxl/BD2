using System;
using System.Collections.Generic;

namespace BD2.Daemon
{
	public class ServiceManager
	{
		ObjectBus objectBus;
		SortedDictionary<Guid, ServiceAnnouncement> localServices = new SortedDictionary<Guid, ServiceAnnouncement> ();
		SortedDictionary<ServiceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, ServiceAgent>> localServiceAgents = new SortedDictionary<ServiceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, ServiceAgent>> ();
		SortedSet<ServiceAnnouncement> remoteServices = new SortedSet<ServiceAnnouncement> ();
		SortedDictionary<Guid, Tuple<ServiceRequest, System.Threading.ManualResetEvent>> requests = new SortedDictionary<Guid, Tuple<ServiceRequest, System.Threading.ManualResetEvent>> ();
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
			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceResponse))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceResponse).FullName));
			ServiceResponse serviceResponse = (ServiceResponse)message;
			Tuple<ServiceRequest, System.Threading.ManualResetEvent> requestTuple = requests [serviceResponse.RequestID];
			lock (pendingResponses)
				pendingResponses.Add (serviceResponse.RequestID, serviceResponse);
			requestTuple.Item2.Set ();
		}

		void ServiceRequestReceived (ObjectBusMessage message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (!(message is ServiceRequest))
				throw new ArgumentException (string.Format ("message type is not valid, must be of type {0}", typeof(ServiceRequest).FullName));
			ServiceRequest serviceRequest = (ServiceRequest)message;
			Func<ServiceAgentMode , ObjectBusSession, ServiceAgent> agentFunc;
			lock (localServices)
				agentFunc = localServiceAgents [localServices [serviceRequest.ServiceID]];
			Guid responseID = Guid.NewGuid ();
			ObjectBusSession session = objectBus.CreateSession (responseID, SessionDisconnected);
			ServiceAgent agent = agentFunc.Invoke (ServiceAgentMode.Server, session);
			lock (sessionAgents)
				sessionAgents.Add (session.SessionID, agent);
			ServiceResponse response = new ServiceResponse (responseID, serviceRequest.ID, ServiceResponseStatus.Accepted);
			objectBus.SendMessage (response);
		}

		void SessionDisconnected (ObjectBusSession session)
		{
			lock (sessionAgents)
				sessionAgents [session.SessionID].SessionDisconnected ();
		}

		void ServiceDestroyReceived (ObjectBusMessage message)
		{
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
			lock (remoteServices)
				return new SortedSet<ServiceAnnouncement> (remoteServices);
		}

		public void AnnounceService (ServiceAnnouncement serviceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, ServiceAgent> func)
		{
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

		public ServiceAgent RequestService (ServiceAnnouncement remoteServiceAnnouncement, Func<ServiceAgentMode , ObjectBusSession, ServiceAgent> func)
		{
			lock (remoteServices)
				if (!remoteServices.Contains (remoteServiceAnnouncement))
					throw new InvalidOperationException ("The provided remoteServiceAnnouncement is not valid.");
			ServiceRequest request = new ServiceRequest (Guid.NewGuid (), remoteServiceAnnouncement.ID);
			System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent (false);
			lock (requests)
				requests.Add (request.ID, new Tuple<ServiceRequest, System.Threading.ManualResetEvent> (request, mre));
			objectBus.SendMessage (request);
			mre.WaitOne ();
			//todo: add exception handling here
			ServiceResponse response = pendingResponses [request.ID];
			lock (pendingResponses)
				pendingResponses.Remove (response.RequestID);
			ServiceAgent agent = func (ServiceAgentMode.Client, objectBus.CreateSession (response.ID, SessionDisconnected));
			sessionAgents.Add (response.ID, agent);
			return agent;
		}
	}
}