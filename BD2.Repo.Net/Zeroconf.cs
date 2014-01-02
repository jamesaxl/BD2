//this code is probably copied from someone elses work or from zeroconf documentation.
//THE LICENSE IS UNKNOWN
//TODO: rewrite from scratch
using System;
using System.Collections.Generic;
using Mono.Zeroconf;
using System.Net;

namespace BD2.Repo.Net
{
	public delegate void ServiceHandler (object o, ServiceArgs args);
	internal class ServiceInfo
	{
		string Name;
		IPAddress IPAddress;
		short port;
		Guid SchemaID;
	}

	internal class ServiceArgs : EventArgs
	{
		private ServiceInfo serviceInfo;

		public ServiceInfo ServiceInfo {
			get { return serviceInfo; }
		}

		public ServiceArgs (ServiceInfo serviceInfo)
		{
			this.serviceInfo = serviceInfo;
		}
	}

	public class ServiceLocator
	{
		private System.Object locker;
		private ServiceBrowser browser;
		private Dictionary <string, ServiceInfo> services;
		private bool showLocals = false;

		public event ServiceHandler ServiceAdded;
		public event ServiceHandler ServiceRemoved;

		public bool ShowLocalServices {
			get { return showLocals; }
			set { showLocals = value; }
		}

		public int Count {
			get {
				int count;
				lock (locker) {
					count = services.Count;
				}
				return count;
			}
		}

		public ServiceInfo[] Services {
			get {
				List<ServiceInfo> serviceList;

				lock (locker) {
					serviceList = new List<ServiceInfo> (services.Values);
				}
				return serviceList.ToArray ();
			}
		}

		public ServiceLocator ()
		{
			locker = new Object ();
			services = new Dictionary <string, ServiceInfo> ();
			Start ();
		}

		public void Start ()
		{
			if (browser == null) {
				browser = new ServiceBrowser ();

				browser.ServiceAdded += OnServiceAdded;
				browser.ServiceRemoved += OnServiceRemoved;

				browser.Browse ("_giver._tcp", "local");
			}
		}

		public void Stop ()
		{
			if (browser != null) {
				lock (locker) {
					services.Clear ();
				}
				browser.Dispose ();
				browser = null;
			}
		}

		private void OnServiceAdded (object o, ServiceBrowseEventArgs args)
		{            
			// Mono.Zeroconf doesn't expose these flags?
			//if ((args.Service.Flags & LookupResultFlags.Local) > 0 && !showLocals)
			//    return;

			args.Service.Resolved += OnServiceResolved;
			args.Service.Resolve ();

			//Logger.Debug("ServiceLocator:OnServiceAdded : {0}", args.Service.Name);
		}

		private void OnServiceResolved (object o, ServiceResolvedEventArgs args)
		{
			IResolvableService service = o as IResolvableService;

			lock (locker) {
				if (services.ContainsKey (service.Name)) {
					// TODO: When making changes (like name or photo) at runtime becomes possible
					// this should allow updates to this info
					return; // we already have it somehow
				}
			}

			ServiceInfo serviceInfo = new ServiceInfo (service.Name, service.HostEntry.AddressList [0], (ushort)service.Port);

			ITxtRecord record = service.TxtRecord;
			serviceInfo.UserName = record ["User Name"].ValueString;
			serviceInfo.MachineName = record ["Machine Name"].ValueString;
			serviceInfo.Version = record ["Version"].ValueString;
			serviceInfo.PhotoType = record ["PhotoType"].ValueString;
			serviceInfo.PhotoLocation = record ["Photo"].ValueString;

			Logger.Debug ("Setting default photo");
			serviceInfo.Photo = Utilities.GetIcon ("blankphoto", 48);

			lock (locker) {
				services [serviceInfo.Name] = serviceInfo;

				if (serviceInfo.PhotoType.CompareTo (Preferences.Local) == 0 ||
					serviceInfo.PhotoType.CompareTo (Preferences.Gravatar) == 0 ||
					serviceInfo.PhotoType.CompareTo (Preferences.Uri) == 0) {
					// Queue the resolution of the photo
					PhotoService.QueueResolve (serviceInfo);
				}              
			}

			if (ServiceAdded != null) {
				Logger.Debug ("About to call ServiceAdded");
				ServiceAdded (this, new ServiceArgs (serviceInfo));
				Logger.Debug ("ServiceAdded was just called");
			} else {
				Logger.Debug ("ServiceAdded was null and not called");
			}
		}

		private void OnServiceRemoved (object o, ServiceBrowseEventArgs args)
		{
			Logger.Debug ("A Service was removed: {0}", args.Service.Name);

			lock (locker) {
				if (services.ContainsKey (args.Service.Name)) {
					ServiceInfo serviceInfo = services [args.Service.Name];
					if (serviceInfo != null)
						services.Remove (serviceInfo.Name);

					if (ServiceRemoved != null)
						ServiceRemoved (this, new ServiceArgs (serviceInfo));
				}
			}
		}
	}
}
	