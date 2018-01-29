using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ForeverFrameChat.Models
{
    public class MessageModel
    {
        public MessageType Type { get; set; }
        public string Value { get; set; }
        public string UserName { get; set; }
        public string Id { get; set; }
    }
    public enum MessageType
    {
        Send,
        Broadcast,
        Join,
        Leave,
        IJoin,
        IsTyping,
        IsTypingPrivate,
        GroopChat,
        IsTypingGroup,
        OnOpen,
        OnClose,
        Reconect
    }
}