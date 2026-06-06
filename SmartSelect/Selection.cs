using Player;
using SlideDrum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BotControl.SmartSelect
{
    public class Selection
    {
        private HashSet<Component> SelectedObjects = new();

        public Selection() { }
        public void Select(Component component, bool oneBot = true)
        {
            SelectedObjects.RemoveWhere(x => x == null);
            if (oneBot && component is PlayerAIBot) // only able to select one bot at a time.
                Deselect<PlayerAIBot>();
            SelectedObjects.Add(component);
        }
        public bool Deselect<T>() where T : Component
        {
            SelectedObjects.RemoveWhere(x => x == null);
            return SelectedObjects.RemoveWhere(obj => obj is T) > 0;
        }
        public bool Deselect(Component component)
        {
            SelectedObjects.RemoveWhere(x => x == null);
            return SelectedObjects.Remove(component);
        }
        public bool Selected<T>(T candidate) where T : Component
        {
            SelectedObjects.RemoveWhere(x => x == null);
            foreach (Component obj in SelectedObjects)
            {
                if (obj == candidate)
                    return true;
            }
            return false;
        }
        public bool Selected<T>() where T : Component
        {
            SelectedObjects.RemoveWhere(x => x == null);
            foreach (Component obj in SelectedObjects)
            {
                if (obj is T)
                    return true;
            }
            return false;
        }
        public HashSet<T> GetSelected<T>() where T : Component
        {
            SelectedObjects.RemoveWhere(x => x == null);
            HashSet<T> ret = new();
            foreach (Component obj in SelectedObjects)
            {
                if (obj is T tObj)
                    ret.Add(tObj);
            }
            return ret;
        }
        public PlayerAIBot GetBestBot()
        {
            SelectedObjects.RemoveWhere(x => x == null);
            return GetSelected<PlayerAIBot>().FirstOrDefault();
        }
        public bool AnyBotsSelected()
        {
            SelectedObjects.RemoveWhere(x => x == null);
            return Selected<PlayerAIBot>();
        }
        public bool AnySelectedBotsAlive()
        {
            SelectedObjects.RemoveWhere(x => x == null);
            if (!AnyBotsSelected())
                return false;
            foreach (PlayerAIBot bot in GetSelected<PlayerAIBot>())
                if (bot?.Agent != null && bot.Agent.Alive)
                    return true;
            return false;
        }
        public bool AnySelectedBotCanReach(Vector3 location)
        {
            SelectedObjects.RemoveWhere(x => x == null);
            if (!AnyBotsSelected())
                return false;
            HashSet<PlayerAIBot> SelectedBots = GetSelected<PlayerAIBot>();
            foreach (PlayerAIBot bot in SelectedBots)
                if (bot.Agent.Alive)
                    if (zHelpers.CanBotReach(bot, location))
                        return true;
            return false;
        }
    }
}
