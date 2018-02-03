using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace ForeverFrameChat.Handlers
{
    public class MyAsyncState : IAsyncResult
    {
        public HttpContext CurrentContext;
        public AsyncCallback AsyncCallback;
        public object ExtraData;

        public string ClientGuid;
        public string ClientName;

        public MyAsyncState(HttpContext context, AsyncCallback callback, object data)
        {
            CurrentContext = context;
            AsyncCallback = callback;
            ExtraData = data;
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return IsCompleted;
            }
        }

        public object AsyncState
        {
            get
            {
                return ExtraData;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return new ManualResetEvent(false);
            }
        }
    }
}