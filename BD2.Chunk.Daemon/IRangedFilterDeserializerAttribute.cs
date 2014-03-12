using System;

namespace BD2.Chunk.Daemon
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class IRangedFilterDeserializerAttribute : Attribute
	{
		System.Reflection.MethodInfo func;

		public IRangedFilter Deserialize (byte[] message)
		{
			return (IRangedFilter)func.Invoke (null, new object[] { message });
		}

		public IRangedFilterDeserializerAttribute (Type type, string funcName)
		{
			if (funcName == null)
				throw new ArgumentNullException ("funcName");
			func = type.GetMethod (funcName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			if (func == null) {
				Console.Write ("ObjectBusMessageDeserializerAttribute is going into failsafe mode, ");
				func = type.GetMethod (funcName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				if (func == null)
					Console.WriteLine ("failed.");
				else
					Console.WriteLine ("succeeded.");
			}
		}
	}
}

