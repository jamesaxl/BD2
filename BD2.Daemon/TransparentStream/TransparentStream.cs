using System;

namespace BD2.Daemon
{
	public class TransparentStream : System.IO.Stream
	{
		ServiceAgent agent;

		public ServiceAgent Agent {
			get {
				return agent;
			}
		}

		Guid streamID;

		public Guid StreamID {
			get {
				return streamID;
			}
		}

		internal TransparentStream (ServiceAgent agent, Guid streamID)
		{
			if (agent == null)
				throw new ArgumentNullException ("agent");
			this.agent = agent;
			this.streamID = streamID;
		}
		#region implemented abstract members of Stream
		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override bool CanRead {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool CanSeek {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool CanWrite {
			get {
				throw new NotImplementedException ();
			}
		}

		public override long Length {
			get {
				throw new NotImplementedException ();
			}
		}

		public override long Position {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion
		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return base.BeginRead (buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return base.BeginWrite (buffer, offset, count, callback, state);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return base.EndRead (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			base.EndWrite (asyncResult);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public override int ReadTimeout {
			get {
				return base.ReadTimeout;
			}
			set {
				base.ReadTimeout = value;
			}
		}

		public override int WriteTimeout {
			get {
				return base.WriteTimeout;
			}
			set {
				base.WriteTimeout = value;
			}
		}

		public override bool CanTimeout {
			get {
				return base.CanTimeout;
			}
		}

		public override void Close ()
		{
			base.Close ();
		}
	}
}

