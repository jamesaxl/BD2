using System;
using BD2.Daemon;

namespace BD2.Conv
{
	public abstract class DatabaseConnectionAgent : BD2.Daemon.ServiceAgent
	{
		protected readonly Guid ListFrontendsMessageType = Guid.Parse ("b2daf4c6-cdf8-487a-900f-88144bae32e8");

		protected DatabaseConnectionAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush, bool run)
			: base(serviceAgentMode, objectBusSession, flush, run)
		{
		}
	}
}

