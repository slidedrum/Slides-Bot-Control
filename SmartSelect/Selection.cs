using Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BotControl.SmartSelect
{
    public class Selection
    {
        private HashSet<Component> SelectedObjects = new(new ComponentInstanceIdComparer());

        public Selection() { }
        private void CleanNull() // TODO this doesn't actually remove null objects
        {
            foreach(var obj in SelectedObjects)
            {
                if (obj == null)
                {
                    SelectedObjects.Remove(obj); // this line is the problem.
                }
            }
        }
        public void Select(Component component, bool oneBot = true)
        {
            CleanNull();
            SelectedObjects.RemoveWhere(x => x == null);
            if (oneBot && component is PlayerAIBot) // only able to select one bot at a time.
                Deselect<PlayerAIBot>();
            SelectedObjects.Add(component);
        }
        public bool Deselect<T>() where T : Component
        {
            CleanNull();
            SelectedObjects.RemoveWhere(x => x == null);
            return SelectedObjects.RemoveWhere(obj => obj is T) > 0;
        }
        public bool Deselect(Component component)
        {
            CleanNull();
            SelectedObjects.RemoveWhere(x => x == null);
            return SelectedObjects.RemoveWhere(obj => obj == component) > 0;
        }
        public bool Selected<T>(T candidate) where T : Component
        {
            CleanNull();
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
            CleanNull();
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
            CleanNull();
            SelectedObjects.RemoveWhere(x => x == null);
            HashSet<T> ret = new();
            foreach (Component obj in SelectedObjects)
            {
                if (obj is T tObj)
                    ret.Add(tObj);
            }
            return ret;
        }
        public PlayerAIBot GetFirstSelectedBot()
        {
            CleanNull();
            SelectedObjects.RemoveWhere(x => x == null);
            return GetSelected<PlayerAIBot>().FirstOrDefault();
        }
        public bool AnyBotsSelected()
        {
            CleanNull();
            SelectedObjects.RemoveWhere(x => x == null);
            return Selected<PlayerAIBot>();
        }
        public bool AnySelectedBotsAlive()
        {
            CleanNull();
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
            CleanNull();
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
