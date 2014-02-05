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

		Action flush;

		protected void Flush ()
		{
			flush ();
		}

		protected ServiceAgent (ServiceAgentMode serviceAgentMode, ObjectBusSession objectBusSession, Action flush)
		{
			if (!Enum.IsDefined (typeof(ServiceAgentMode), serviceAgentMode)) {
				throw new ArgumentException ("Invalid value for argument 'serviceAgentMode'", "serviceAgentMode");
			}
			if (objectBusSession == null)
				throw new ArgumentNullException ("objectBusSession");
			if (flush == null)
				throw new ArgumentNullException ("flush");
			this.serviceAgentMode = serviceAgentMode;
			this.objectBusSession = objectBusSession;
			this.flush = flush;
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