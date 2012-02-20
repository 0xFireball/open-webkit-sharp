﻿// The ResourcesIntercepter class aims to provide you with events
// related to the loading of resources (HTML, CSS, JS etc.) from
// a WebView object. It is based on the WebResourceLoadDelegate class.

using System;
using System.Collections.Generic;
using System.Text;
using WebKit.Interop;

namespace WebKit
{
    public class WebKitResource
    {
        private int _size = -1;
        public string Url { get; internal set; }
        public string MimeType { get; internal set; }
        public string Encoding { get; internal set; }
        public long Size
        {
            get
            {
                if (_size != -1)
                    return _size;
                else
                {
                    System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
                    System.Net.WebResponse resp = req.GetResponse();
                    return resp.ContentLength;
                }
            }
        }
        public WebKitResource(IWebResource resource)
        {
            this.Url = resource.url();
            this.MimeType = resource.mimeType();
            this.Encoding = resource.textEncodingName();
        }
        public WebKitResource(WebURLResponse res, int size)
        {
            this.Url = res.url();
            this.MimeType = res.mimeType();
            this.Encoding = res.textEncodingName();
        }
        public WebKitResource(WebURLResponse res)
        {
            this.Url = res.url();
            this.MimeType = res.mimeType();
            this.Encoding = res.textEncodingName();
        }
    }
    public class WebKitResourceErrorEventArgs : EventArgs
    {
        public WebKitResource Resource { get; internal set; }
        public string ErrorDescription { get; internal set; }
        public WebKitResourceErrorEventArgs(WebURLResponse res, string error)
        {
            this.Resource = new WebKitResource(res);
            this.ErrorDescription = error;
        }
    }
    public class WebKitResourcesEventArgs : EventArgs
    {
        public WebKitResource Resource { get; internal set; }
        public WebKitResourcesEventArgs(WebURLResponse res)
        {
            this.Resource = new WebKitResource(res);
        }
    }
    public class WebKitLoadingResourceEventArgs : EventArgs
    {
        public WebKitResource Resource { get; internal set; }
        public int Received { get; internal set; }
        public WebKitLoadingResourceEventArgs(WebURLResponse res, int length)
        {
            this.Resource = new WebKitResource(res);
            this.Received = length;
        }
    }
    public class WebKitResourceRequestEventArgs : EventArgs
    {
        public bool SendRequest { get; set; }
        public string ResourceUrl { get; set; }
        public WebKitResourceRequestEventArgs(string u, bool s)
        {
            this.SendRequest = s;
            this.ResourceUrl = u;
        }
    }
    public delegate void ResourceStartedLoadingHandler(object sender, WebKitResourcesEventArgs e);
    public delegate void ResourceFinishedLoadingHandler(object sender, WebKitResourcesEventArgs e);
    public delegate void ResourceFailedHandler(object sender, WebKitResourceErrorEventArgs e);
    public delegate void ResourceSizeAvailable(object sender, WebKitLoadingResourceEventArgs e);
    public delegate void ResourceSendRequestEventHandler(object sender, WebKitResourceRequestEventArgs e);
    public class ResourcesIntercepter
    {
        public event ResourceStartedLoadingHandler ResourceStartedLoadingEvent;
        public event ResourceFinishedLoadingHandler ResourceFinishedLoadingEvent;
        public event ResourceSizeAvailable ResourceSizeAvailableEvent;
        public event ResourceSendRequestEventHandler ResourcesSendRequest;
        public event ResourceFailedHandler ResourceFailedLoading;
        public WebKitBrowser Owner;
        public List<WebKitResource> Resources;
        public ResourcesIntercepter(WebKitBrowser browser)
        {
            this.Owner = browser;
            Resources = new List<WebKitResource>();
        }
        internal void ResFailed(WebURLResponse res, string des)
        {
            try
            {
                if (Owner.WebView != null)
                    ResourceFailedLoading(this, new WebKitResourceErrorEventArgs(res, des));
            }
            catch { }
        }
        internal void ResStart(WebURLResponse res)
        {
            try
            {
                if (Owner.WebView != null)
                {
                    ResourceStartedLoadingEvent(this, new WebKitResourcesEventArgs(res));
                }
            }
            catch { }
        }
        internal void ResFinish(WebURLResponse res)
        {
            try
            {
                if (Owner.WebView != null)
                    ResourceFinishedLoadingEvent(this, new WebKitResourcesEventArgs(res));
                Resources.Add(new WebKitResource(res, (int)res.expectedContentLength()));
            }
            catch { }
        }
        internal void ResProg(WebURLResponse res, int r)
        {
                if (Owner.WebView != null)
                    ResourceSizeAvailableEvent(this, new WebKitLoadingResourceEventArgs(res, r));
        }
        internal string ResReq(string url)
        {
            if (string.IsNullOrEmpty(url) == false && this.ResourcesSendRequest != null)
            {
                WebKitResourceRequestEventArgs args = new WebKitResourceRequestEventArgs(url, true);
                ResourcesSendRequest(this, args);
                if (args.SendRequest == false)
                    return null;
                else
                    return args.ResourceUrl;
            }
            else
                return url;
        }
    }
}
