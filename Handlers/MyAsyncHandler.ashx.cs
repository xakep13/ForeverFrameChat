using ForeverFrameChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;

namespace ForeverFrameChat.Handlers
{
    public class MyAsyncHandler : IHttpAsyncHandler, System.Web.SessionState.IReadOnlySessionState
    {
        public IAsyncResult BeginProcessRequest(HttpContext ctx, AsyncCallback cb, Object obj)
        {
            ctx.ThreadAbortOnTimeout = false;

            MyAsyncState currentAsyncState = new MyAsyncState(ctx, cb, obj);

            ThreadPool.QueueUserWorkItem(new WaitCallback(RequestWorker), currentAsyncState);

            return currentAsyncState;
        }

        public void EndProcessRequest(IAsyncResult ar) { }

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context) { }

        private void RequestWorker(Object obj)
        {
            MyAsyncState state = obj as MyAsyncState;
            state.CurrentContext.Response.Write(FirstText());

            var message = state.CurrentContext.Request.QueryString["msg"];
            var id = state.CurrentContext.Request.QueryString["id"];
            JavaScriptSerializer ser = new JavaScriptSerializer();
            if (message != null)
            {
                var msg = ser.Deserialize<MessageModel>(message);
                ClientProcesor.UpdateClient(state, id);

                switch (msg.Type)
                {
                    case MessageType.OnOpen:
                        ClientProcesor.OnOpen(msg, state);
                        break;
                    case MessageType.OnClose:
                        ClientProcesor.OnClose(state);
                        break;
                    case MessageType.Broadcast:
                        ClientProcesor.SendPublicMessage(msg, state);
                        break;
                    case MessageType.Join:
                        ClientProcesor.SendJoinMessage(msg, state);
                        break;
                    case MessageType.Send:
                        ClientProcesor.SendPrivateMessage(msg, state);
                        break;
                    case MessageType.IsTyping:
                        ClientProcesor.SayWhoIsTyping(msg, state);
                        break;
                    case MessageType.IsTypingPrivate:
                        ClientProcesor.SayWhoIsTypingPrivate(msg, state);
                        break;
                    case MessageType.GroopChat:
                        ClientProcesor.SendGroupMessage(msg, state);
                        break;
                    case MessageType.IsTypingGroup:
                        ClientProcesor.SayWhoIsTypingGroup(msg, state);
                        break;
                    case MessageType.Reconect:
                        // ClientProcesor.Reconect(state);
                        break;
                }
            }
        }

        public static string FirstText()
        {
            string s = "";
            for (int j = 0; j < 100; j++) s += "*";
            return " <!DOCTYPE HTML><html>" +
                    "<head><meta junk = '" + s + "'/> " +
                        "<script> " +
                        "var i = parent.Iframe; " +
                        "i.onConnected(); </script> " +
                     "</head><body >";
        }
    }
}