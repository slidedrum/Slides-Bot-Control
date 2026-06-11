using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BotControl.SmartSelect.PressTypes
{
    public static class PressTypeManager
    {
        private static Dictionary<string, IPressType> _TypeMap = null;
        public static Dictionary<string, IPressType> TypeMap 
        { 
            get 
            {
                if (_TypeMap == null)
                    Initalize();
                return _TypeMap;
            } 
        } // used to get lookup instances.
        public static bool initalized => _TypeMap != null; // Does Typemap need to be created?
        public static IPressType GetPressType(string FriendlyName) // Used to get a press type by its friendly name, returns null if not found
        {
            if (TypeMap == null)
                Initalize();
            if (TypeMap.TryGetValue(FriendlyName, out IPressType pressType))
                return pressType;
            return null;
        }
        public static HashSet<IPressType> GetAllPressTypes()
        {
            return TypeMap.Values.ToHashSet();
        }
        private static void Initalize() // Find all press types using reflection and create instances of them, then call their OnRegister method
        {
            if (initalized)
                return;
            _TypeMap = new Dictionary<string, IPressType>();
            Type baseType = typeof(IPressType);
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));
            foreach (Type type in types)
            {
                IPressType instance = (IPressType)Activator.CreateInstance(type, nonPublic: true);
                string key = instance.FriendlyName;
                if (_TypeMap.ContainsKey(key))
                    throw new Exception($"Duplicate PressAction key: {key}");
                _TypeMap[key] = instance;
                instance.OnRegister();
            }
        }
        public static void Update() // Call the update method of all press types
        {
            if (!initalized)
                Initalize();
            foreach (IPressType pressType in TypeMap.Values)
            {
                pressType.Update();
            }
        }
    }
}
