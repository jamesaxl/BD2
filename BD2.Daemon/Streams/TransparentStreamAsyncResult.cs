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

namespace BD2.Daemon.Streams
{
	sealed class TransparentStreamAsyncResult : IAsyncResult
	{
		readonly System.Threading.ManualResetEvent waitHandle = new System.Threading.ManualResetEvent (false);
		readonly object asyncState;
		TransparentStreamMessageBase response;
		readonly AsyncCallback callback;

		public TransparentStreamMessageBase Response {
			get {
				if (!IsCompleted) {
					waitHandle.WaitOne ();
				}
				return response;
			}
		}

		internal TransparentStreamAsyncResult (AsyncCallback callback, object asyncState)
			: this (asyncState)
		{
			this.callback = callback;
		}

		internal TransparentStreamAsyncResult (object asyncState)
		{
			this.asyncState = asyncState;
		}

		#region IAsyncResult implementation

		public object AsyncState {
			get {
				return asyncState;
			}
		}

		public System.Threading.WaitHandle AsyncWaitHandle {
			get {
				return waitHandle;
			}
		}

		public  bool CompletedSynchronously {
			get {
				//never happens
				return false;
			}
		}

		public bool IsCompleted {
			get {
				return response != null;
			}
		}

		#endregion

		internal void Set (TransparentStreamMessageBase transparentStreamMessageBase)
		{
			response = transparentStreamMessageBase;
			waitHandle.Set ();
			if (callback != null)
				callback (this);
		}
	}
}
