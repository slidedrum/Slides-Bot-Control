using BotControl.Menus;
using Player;
using UnityEngine;

namespace BotControl
{
    public struct ChatMessage
    {
        public int? SpeakerID;
        public int? ReceiverID;
        public string Message;
        public float localtimestamp;
        private uint _Hash;
        public uint Hash 
        { 
            get 
            {
                if (_Hash == 0)
                    _Hash = zHelpers.HashString(Message + SpeakerID.ToString() + ReceiverID.ToString() + localtimestamp.ToString());
                return _Hash;
            } 
        }
        public ChatMessage(string Message, int? SpeakerID, int? ReceiverID)
        {
            localtimestamp = Time.time;
            this.Message = Message;
            this.SpeakerID = SpeakerID;
            this.ReceiverID = ReceiverID;
        }
    }
    internal static class zChatHandler
    {

        private static ChatMessage previousMessage = new();
        public static void sendChatMessage(string message, string PermissionString, PlayerAgent sender = null, PlayerAgent receiver = null)
        {
            if (!(bool)zSlideComputer.ActionPermissions.ValueAt(PermissionString))
                return;
            ChatMessage ThisMessage = new()
            {
                Message = message,
                SpeakerID = sender?.CharacterID,
                ReceiverID = receiver?.CharacterID,
            };
            bool same = ThisMessage.Hash == previousMessage.Hash;
            previousMessage = ThisMessage;
            if (same)
                return;
            if ((bool)zSlideComputer.ActionPermissions.ValueAt("TalkInChat"))
                PlayerChatManager.WantToSentTextMessage(sender != null ? sender : PlayerManager.GetLocalPlayerAgent(), message, receiver);
        }
    }
}
