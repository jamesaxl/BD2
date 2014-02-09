using System;

namespace BD2.Daemon
{
	class TransparentStreamAsyncResult : IAsyncResult
	{
		ServiceAgent agent;
		TransparentStream transparentStream;
		ObjectBusMessage requestMessage;
		System.Threading.ManualResetEvent waitHandle = new System.Threading.ManualResetEvent (false);
		object asyncState;

		public TransparentStream TransparentStream {
			get {
				return transparentStream;
			}
		}

		internal TransparentStreamAsyncResult (object asyncState)
		{
			if (asyncState == null)
				throw new ArgumentNullException ("asyncState");
			this.asyncState = asyncState;
		}
		#region IAsyncResult implementation
		object IAsyncResult.AsyncState {
			get {
				return asyncState;
			}
		}

		System.Threading.WaitHandle IAsyncResult.AsyncWaitHandle {
			get {
				return waitHandle;
			}
		}

		bool IAsyncResult.CompletedSynchronously {
			get {
				//never happens
				return false;
			}
		}

		bool IAsyncResult.IsCompleted {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}
