using System;

namespace BD2.Conv.Frontend.Table
{
	public class ServiceAgent : BD2.Daemon.ServiceAgent
	{
		#region implemented abstract members of ServiceAgent
		protected override void Run ()
		{
			throw new NotImplementedException ();
		}

		public override void DestroyRequestReceived ()
		{
			throw new NotImplementedException ();
		}

		public override void SessionDisconnected ()
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}

