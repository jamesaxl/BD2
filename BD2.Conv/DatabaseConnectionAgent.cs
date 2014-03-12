using System;

namespace BD2.Conv
{
	public abstract class DatabaseConnectionAgent : BD2.Daemon.ServiceAgent
	{
		protected readonly Guid ListFrontendsMessageType = Guid.Parse ("");
		#region Abstract members of ServiceAgent
		protected abstract void Run ();

		public abstract void DestroyRequestReceived ();

		public abstract void SessionDisconnected ();
		#endregion
	}
}

