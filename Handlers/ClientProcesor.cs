using ForeverFrameChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ForeverFrameChat.Handlers
{
    public class ClientProcesor
    {
        private static Object _lock = new Object();
        private static List<MyAsyncState> _clientStateList = new List<MyAsyncState>();
        private static List<MessageModel> CurrentMessage = new List<MessageModel>();
        private static JavaScriptSerializer ser = new JavaScriptSerializer();
        private static List<UserModel> ConnectedUsers = new List<UserModel>();

        public static void Send(string message, MyAsyncState state)
        {
            lock (_lock)
            {
                try
                {
                    state.CurrentContext.Response.Write("<script>i.onMessage('" + message + "'); </script>");
                    state.CurrentContext.Response.Flush();
                    state.CurrentContext.Response.Write("<script>console.log('ok'); </script>");
                    state.CurrentContext.Response.Flush();
                }
                catch
                {
                    try
                    {
                        Reconect(state);
                    }
                    catch
                    {
                        OnClose(state);
                    }
                }
            }
        }

        private static void Reconect(MyAsyncState state)
        {
            state.CurrentContext.Response.Write(MyAsyncHandler.FirstText());
            state.CurrentContext.Response.Flush();
        }

        public static void Broadcast(string message)
        {
            lock (_lock)
            {
                foreach (MyAsyncState state in _clientStateList.ToList())
                {
                    Send(message, state);
                }
            }
        }

        public static void Broadcast(string message, MyAsyncState state1)
        {
            foreach (MyAsyncState state in _clientStateList.ToList())
            {
                if (state != state1)
                    Send(message, state);
            }
        }

        public static void UpdateClient(MyAsyncState state, string id)
        {
            MyAsyncState clientState = _clientStateList.Find(s => s.ClientGuid == id);
            if (clientState != null)
            {
                clientState.CurrentContext = state.CurrentContext;
                clientState.ExtraData = state.ExtraData;
                clientState.AsyncCallback = state.AsyncCallback;
            }
        }

        public static void OnOpen(MessageModel msg, MyAsyncState state)
        {
            string id = msg.Id;
            string name = msg.UserName;
            RegisterClient(state, id, name);

            Send(ser.Serialize(new
            {
                Type = MessageType.IJoin,
                UserName = name,
                Value = CurrentMessage,
                Id = ConnectedUsers
            }), state);
            Broadcast(ser.Serialize(new
            {
                Type = MessageType.Join,
                UserName = msg.UserName,
                Value = "Приєднався",
                Id = id
            }), state);
        }

        public static void OnClose(MyAsyncState state)
        {
            Broadcast(ser.Serialize(new
            {
                Type = MessageType.Leave,
                UserName = state.ClientName,
                Value = "Покинув бесіду",
                Id = state.ClientGuid
            }), state);
            UnregisterClient(state);
        }

        public static void SendJoinMessage(MessageModel msg, MyAsyncState state)
        {
            Broadcast(ser.Serialize(new
            {
                Type = MessageType.Join,
                UserName = msg.UserName,
                Value = "Приєднався",
                Id = msg.Id
            }), state);
        }

        public static void SayWhoIsTyping(MessageModel msg, MyAsyncState state)
        {
            Broadcast(ser.Serialize(new
            {
                Type = MessageType.IsTyping,
                UserName = msg.UserName,
                Value = msg.UserName + " is typing...",
                Id = msg.Id
            }), state);
        }

        public static void SayWhoIsTypingPrivate(MessageModel msg, MyAsyncState state)
        {
            MyAsyncState channel = _clientStateList.Find(n => n.ClientGuid == msg.Id);
            Send(ser.Serialize(new
            {
                Type = MessageType.IsTypingPrivate,
                UserName = msg.UserName,
                Value = msg.UserName + " is typing...",
                Id = msg.Id
            }), channel);
        }

        public static void SendGroupMessage(MessageModel msg, MyAsyncState state)
        {
            string[] users = msg.Id.Split(' ');

            foreach (string user in users)
            {
                var chanel = _clientStateList.Find(n => n.ClientGuid == user);
                if (chanel != null)
                {
                    Send(ser.Serialize(new
                    {
                        Type = MessageType.GroopChat,
                        UserName = msg.UserName,
                        Value = msg.Value,
                        Id = msg.Id
                    }), chanel);
                }
            }
        }

        public static void SayWhoIsTypingGroup(MessageModel msg, MyAsyncState state)
        {
            string[] users = msg.Id.Split(' ');

            foreach (string user in users)
            {
                var channel = _clientStateList.Find(n => n.ClientGuid == user);
                if (channel != null)
                {
                    Send(ser.Serialize(new
                    {
                        Type = MessageType.IsTypingGroup,
                        UserName = msg.UserName,
                        Value = msg.UserName + " is typing...",
                        Id = msg.Id
                    }), channel);
                }
            }

        }

        public static void SendPrivateMessage(MessageModel msg, MyAsyncState state)
        {
            string[] var1 = Convert.ToString(msg.Id).Split(' ');
            string from = var1[1];
            string to = var1[0];

            var channel = _clientStateList.FirstOrDefault(n => n.ClientGuid == to);
            var myChannel = _clientStateList.FirstOrDefault(n => n.ClientGuid == from);

            if (channel != null && myChannel != null)
            {
                Send(ser.Serialize(new
                {
                    Type = msg.Type,
                    UserName = msg.UserName,
                    Value = msg.Value,
                    Id = from
                }), channel);
                Send(ser.Serialize(new
                {
                    Type = msg.Type,
                    UserName = msg.UserName,
                    Value = msg.Value,
                    Id = to
                }), myChannel);
            }
        }

        public static void SendPublicMessage(MessageModel msg, MyAsyncState state)
        {
            Broadcast(ser.Serialize(new
            {
                Type = msg.Type,
                UserName = msg.UserName,
                Value = msg.Value,
                Id = msg.Id
            }));
            AddMessageinCache(msg.UserName, msg.Value);
        }

        public static void AddMessageinCache(string name, string value)
        {
            CurrentMessage.Add(new MessageModel { UserName = name, Value = value });

            if (CurrentMessage.Count > 100)
                CurrentMessage.RemoveAt(0);
        }

        public static void RegisterClient(MyAsyncState state, string id, string name)
        {
            lock (_lock)
            {
                state.ClientName = name;
                state.ClientGuid = id;
                _clientStateList.Add(state);
                ConnectedUsers.Add(new UserModel { UserName = name, ConnectionId = id });
            }
        }

        public static void UnregisterClient(MyAsyncState state)
        {
            lock (_lock)
            {
                _clientStateList.Remove(state);
                UserModel user = ConnectedUsers.Find(x => x.ConnectionId == state.ClientGuid);
                ConnectedUsers.Remove(user);

            }
        }
    }
}