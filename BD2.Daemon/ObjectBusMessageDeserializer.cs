using System;

namespace BD2.Daemon
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ObjectBusMessageDeserializerAttribute : Attribute
	{
		System.Reflection.MethodInfo func;

		public ObjectBusMessage Deserialize (byte[] message)
		{
			return (ObjectBusMessage)func.Invoke (null, new object[] { message });
		}

		public ObjectBusMessageDeserializerAttribute (Type type, string funcName)
		{
			if (funcName == null)
				throw new ArgumentNullException ("funcName");
			func = type.GetMethod (funcName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			if (func == null) {
				Console.WriteLine ("ObjectBusMessageDeserializerAttribute is going into failsafe mode.");
				func = type.GetMethod (funcName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				if (func == null)
					Console.WriteLine ("failed.");
				else
					Console.WriteLine ("succeeded.");
			}
		}
	}
}

