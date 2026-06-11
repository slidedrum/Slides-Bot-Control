using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BotControl.SmartSelect.PressActions
{

    public static class PressActionManager
    {
        private static Dictionary<string, IPressAction> ActionMap;
        public static void Initialize()
        {
            if (ActionMap != null)
                return;
            ActionMap = new Dictionary<string, IPressAction>();
            var baseType = typeof(IPressAction);
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));
            foreach (var type in types)
            {
                var instance = (IPressAction)Activator.CreateInstance(type, nonPublic: true);
                var key = instance.FriendlyName;
                if (ActionMap.ContainsKey(key))
                    throw new Exception($"Duplicate PressAction key: {key}");
                ActionMap[key] = instance;
                instance.Register();
            }
        }
        public static IPressAction GetAction(string name)
        {
            if (ActionMap == null)
                Initialize();

            if (!ActionMap.TryGetValue(name, out IPressAction action))
                ZiMain.log.LogError($"Could not find action {name} in Press Action Map.");
            return action;
        }
        public static HashSet<IPressAction> GetAllActions()
        {
            return ActionMap.Values.ToHashSet();
        }
    }
}
