using System;

namespace BD2.Daemon
{
	public abstract class ServiceAgent
	{
		ServiceAgentMode serviceAgentMode;

		protected ServiceAgentMode ServiceAgentMode {
			get {
				return serviceAgentMode;
			}
		}

		ObjectBusSession objectBusSession;

		protected ObjectBusSession ObjectBusSession {
			get {
				return objectBusSession;
			}
		}

		System.Threading.Thread thread;

		protected System.Threading.Thread Thread {
			get {
				return thread;
			}
		}

		protected ServiceAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession)
		{
			if (!Enum.IsDefined (typeof(ServiceAgentMode), serviceAgentMode)) {
				throw new ArgumentException ("Invalid value for argument 'serviceAgentMode'", "serviceAgentMode");
			}
			if (objectBusSession == null)
				throw new ArgumentNullException ("objectBusSession");

			this.serviceAgentMode = serviceAgentMode;
			this.objectBusSession = objectBusSession;
			thread = new System.Threading.Thread (Run);
			thread.Start ();
		}

		public void Destroy ()
		{
			ObjectBusSession.Destroy ();
		}

		protected abstract void Run ();

		public abstract void DestroyRequestReceived ();

		public abstract void SessionDisconnected ();
	}
}
