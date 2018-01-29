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
        private Boolean _isCompleted;

        public MyAsyncState(HttpContext context, AsyncCallback callback, object data)
        {
            CurrentContext = context;
            AsyncCallback = callback;
            ExtraData = data;
            _isCompleted = false;
        }

        public void CompleteRequest()
        {
            _isCompleted = true;
            AsyncCallback?.Invoke(this);
        }

        #region IAsyncResult Members

        public Boolean CompletedSynchronously
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
                return _isCompleted;
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

        WaitHandle IAsyncResult.AsyncWaitHandle => throw new NotImplementedException();
  
        #endregion
    }
}