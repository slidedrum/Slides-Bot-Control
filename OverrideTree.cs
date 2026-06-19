//using BotControl.Networking;
using FlexMethodDefinition;
//using GTFO.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UnityEngine;
namespace SlideDrum
{
    public interface IOverrideTree
    {
        public bool IsDefaultValue(string key);
        public bool IHasDefault(string key);
        public bool IMatchingDefaultValue(string key);
        public bool IHasKey(string key);
        public bool IHasValue(string key);
        public bool IHasParrent(string key);
        public object? IGetValue(string key);
        public object? IValueAt(string key);
        public object? IGetDefaultValue(string key);
        public uint treeID { get; }
        public string identifier { get; }
        public bool AllDefault(IEnumerable<string> keys);


    }

    public class OverrideTree<T> : IOverrideTree
    {
        public static event Action<IOverrideTree, uint, string, object?, ulong> OnValueSet;
        internal static Dictionary<uint, OverrideTree<T>> Trees = new();
        public uint treeID;
        private string identifier = "DefaultIdent";
        public Type type;
        public Dictionary<uint, Node> nodesByID { get; private set; } = new(); //For O(1) lookup by ID, used for network syncing
        public Dictionary<string, Node> nodes { get; private set; } = new(StringComparer.Ordinal); //For O(1) lookup, starting search in the middle of a tree
        public Node rootNode { get; private set; }

        string IOverrideTree.identifier => identifier;
        uint IOverrideTree.treeID => treeID;
        public class FallbackLink
        {
            public Node Target { get; }
            public int? MaxDepth { get; }

            public FallbackLink(Node target, int? maxDepth = null)
            {
                Target = target ?? throw new ArgumentNullException(nameof(target));
                MaxDepth = maxDepth;
            }
        }

        public class Node
        {
            public struct NodeIdentity<T>
            {
                public bool? hasValue { get; set; }
                public bool? hasDefaultValue { get; set; }
                public T Value { get; set; }
                public T DefaultValue { get; set; }
            }
            public NodeIdentity<T> Identity => new()
            {
                hasValue = HasValue(),
                hasDefaultValue = hasDefaultValue,
                Value = Value,
                DefaultValue = DefaultValue,
            };
            public uint nodeID { get; private set; }
            public string nodeIdentity { get; private set; }
            public T? Value { get; set; }
            private bool hasDefaultValue { get; set; }
            private T? _DefaultValue { get; set; }
            public T? DefaultValue
            {
                get
                {
                    if (hasDefaultValue)
                        return _DefaultValue;
                    if (Parent != null)
                        return Parent.DefaultValue;
                    return default(T);
                }
            }
            public T? InitalValue { get; }
            public FlexibleEvent onChanged = new();
            public FlexibleEvent onThisNodeChanged = new();
            public Func<bool>? Condition { get; }
            public bool IsRoot => Parent == null;
            public Node? Parent { get; private set; }
            public OverrideTree<T> Tree { get; internal set; }
            public List<Node> Children { get; } = new();
            public List<FallbackLink> Fallbacks { get; } = new();
            internal Node(string key, Node parent = null, T? value = default, Func<bool>? condition = null, T? defaultValue = default, bool hasDefaultValue = false) //If you supply a parent, you can opt to not supply a value
            {
                if (defaultValue != null)
                    hasDefaultValue = true;
                nodeIdentity = key;
                Value = value;
                Condition = condition;
                Parent = parent;
                InitalValue = value;
                _DefaultValue = defaultValue;
                nodeID = HashString(GetNodeTreeString());
                this.hasDefaultValue = hasDefaultValue;
            }
            public T? GetValue() //Traverse down the tree to get deepest value
            {
                T? ret = ValueAt();
                foreach (var node in Children)
                {
                    if (node.Condition != null && !node.Condition.Invoke())
                        continue;
                    var childValue = node.GetValue();
                    if (childValue != null)
                    {
                        ret = childValue;
                    }
                }
                return ret;
            }
            public void AddFallback(Node target, int? maxDepth = null)
            {
                if (target == null)
                    throw new ArgumentNullException(nameof(target));
                Fallbacks.Add(new FallbackLink(target, maxDepth));
            }

            public bool RemoveFallback(Node target, int? maxDepth = null, bool matchMaxDepth = false)
            {
                if (target == null)
                    throw new ArgumentNullException(nameof(target));
                for (int i = 0; i < Fallbacks.Count; i++)
                {
                    var link = Fallbacks[i];
                    if (link.Target != target)
                        continue;
                    if (matchMaxDepth && link.MaxDepth != maxDepth)
                        continue;
                    Fallbacks.RemoveAt(i);
                    return true;
                }
                return false;
            }

            public T? ValueAt() //Traverse up the tree to get value at given node
            {
                if (Value != null) return Value;

                foreach (var fallback in Fallbacks)
                {
                    var fallbackValue = ResolveFallbackValue(fallback, new HashSet<Node>());
                    if (fallbackValue != null)
                        return fallbackValue;
                }

                if (Parent == null)
                    throw new InvalidOperationException("Root node has null value.");
                return Parent.ValueAt();
            }

            private T? ResolveFallbackValue(FallbackLink link, HashSet<Node> visited)
            {
                return ResolveFallbackNode(link.Target, link.MaxDepth, visited);
            }

            private T? ResolveFallbackNode(Node node, int? maxDepth, HashSet<Node> visited)
            {
                if (node == null)
                    return default;

                if (!visited.Add(node))
                    return default;

                if (node.Value != null)
                    return node.Value;

                if (maxDepth == 0)
                    return default;

                int? childBudget = maxDepth.HasValue ? maxDepth.Value - 1 : null;

                foreach (var fallback in node.Fallbacks)
                {
                    int? effectiveDepth = CombineMaxDepth(childBudget, fallback.MaxDepth);
                    var value = ResolveFallbackNode(fallback.Target, effectiveDepth, visited);
                    if (value != null)
                        return value;
                }

                return default;
            }

            private static int? CombineMaxDepth(int? budget, int? linkMaxDepth)
            {
                if (budget == null)
                    return linkMaxDepth;
                if (linkMaxDepth == null)
                    return budget;
                return Math.Min(budget.Value, linkMaxDepth.Value);
            }
            public void SetValue(T? newValue)
            {
                var callList = OnChanged();
                Value = newValue;
                onThisNodeChanged.Invoke();

                foreach (var call in callList)
                    call.Invoke();
            }
            private HashSet<FlexibleEvent> OnChanged(bool fromParrent = false, HashSet<FlexibleEvent> callList = null)
            {
                if (callList == null)
                    callList = new HashSet<FlexibleEvent>();
                if (Value != null && fromParrent)
                    return callList;
                callList.Add(onChanged);
                foreach (var child in Children)
                    child.OnChanged(true, callList);
                return callList;
            }
            public bool HasValue()
            {
                return Value != null;
            }
            public bool HasParrent()
            {
                return Parent != null;
            }
            public bool HasDefault()
            {
                return hasDefaultValue;
            }
            public void SetDefaultValue(T? defaultValue)
            {
                _DefaultValue = defaultValue;
                SetHasDefaultValue(true);
            }
            public void SetHasDefaultValue(bool hasDefaultValue)
            {
                this.hasDefaultValue = hasDefaultValue;
            }
            public bool MatchingDefaultValue()
            {
                if (!HasValue())
                    return true;
                if (hasDefaultValue && DefaultValue != null)
                    return EqualityComparer<T?>.Default.Equals(DefaultValue, Value);
                if (Parent == null)
                    return IsDefaultValue();
                return EqualityComparer<T?>.Default.Equals(Parent.ValueAt(), Value);
            }
            public bool IsDefaultValue()
            {
                if (HasValue() == false)
                    return true;
                return EqualityComparer<T?>.Default.Equals(DefaultValue, Value);
            }
            internal string GetNodeTreeString()
            {
                if (Parent == null)
                    return nodeIdentity;
                return Parent.GetNodeTreeString() + "/" + nodeIdentity;
            }
        }
        public static uint HashString(string str)
        {
            unchecked
            {
                uint hash = 2166136261;

                for (int i = 0; i < str.Length; i++)
                {
                    hash ^= str[i];
                    hash *= 16777619;
                }

                return hash;
            }
        }
        public static void ResetTrees()
        {
            Trees.Clear();
        }
        public string GetNodesSerialized()
        {
            return JsonSerializer.Serialize(nodes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Identity));
        }
        public void SetNodesSerialized(string json)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, Node.NodeIdentity<T>>>(json);
            foreach (var kvp in dict)
            {
                string identifer = kvp.Key;
                Node.NodeIdentity<T> identity = kvp.Value;
                if (identity.hasValue != null && (bool)identity.hasValue)
                {
                    SetValue(identifer, identity.Value);
                }
                if (identity.hasDefaultValue != null && (bool)identity.hasDefaultValue)
                {
                    SetDefaultValue(identifer, identity.DefaultValue);
                }
            }
        }
        public OverrideTree(T rootValue, string identifier, string rootKey = "Default", FlexibleMethodDefinition OnChanged = null)
        {
            this.identifier = identifier;
            treeID = HashString(identifier);
            Trees[treeID] = this;
            if (rootValue == null)
                throw new ArgumentNullException(nameof(rootValue), "Initial value can not be null");
            rootNode = new Node(rootKey, value: rootValue, defaultValue: rootValue);
            nodes[rootKey] = rootNode;
            nodesByID[0] = rootNode;
            if (OnChanged != null)
                rootNode.onChanged.Listen(OnChanged);
            type = typeof(T);
        }
        public static OverrideTree<T> GetTreeFromID(uint ID)
        {
            return Trees[ID];
        }
        public Node GetNodeFromIdent(string ident)
        {
            if (!nodes.ContainsKey(ident))
                throw new KeyNotFoundException(nameof(ident));
            return nodes[ident];
        }
        public Node GetNodeFromId(uint id)
        {
            return nodesByID[id];
        }
        public Node AddNode(string key, T? value, string? parent = null, Func<bool>? condition = null, FlexibleMethodDefinition onChanged = null, T? defaultValue = default, bool hasDefaultValue = false)
        {
            if (defaultValue != null)
                hasDefaultValue = true;
            Node parrentNode = null;
            if (parent != null)
            {
                if (!nodes.ContainsKey(parent))
                    throw new KeyNotFoundException($"Could not find parrent named {parent} when adding node {key}");
                parrentNode = nodes[parent];
            }

            return AddNode(key, value, parrentNode, condition, onChanged, defaultValue, hasDefaultValue);
        }
        public Node AddNode(string key, T? value, Node? parent = null, Func<bool>? condition = null, FlexibleMethodDefinition onChanged = null, T? defaultValue = default, bool hasDefaultValue = false)
        {
            if (defaultValue != null)
                hasDefaultValue = true;
            if (nodes.ContainsKey(key))
                if (parent == null)
                    throw new InvalidOperationException($"Key '{key}' already in use.");
                else
                    throw new InvalidOperationException($"Key '{key}' already in use. Consider combineing with the parrent key for '{parent.nodeIdentity}/{key}'");

            if (parent == null)
                parent = rootNode;
            if (!nodes.Values.Contains(parent))
                throw new InvalidOperationException($"Parent '{parent.nodeIdentity}' not found.");

            var node = new Node(key, parent, value, condition, defaultValue, hasDefaultValue);
            parent.Children.Add(node);
            nodes[key] = node;
            nodesByID[node.nodeID] = node;
            node.Tree = this;
            if (onChanged != null)
                node.onChanged.Listen(onChanged);
            return node;
        }
        public OverrideTree<T> AddFallback(string key, string targetKey, int? maxDepth = null)
        {
            AddFallback(NodeAt(key), GetNodeFromIdent(targetKey), maxDepth);
            return this;
        }
        public Node AddFallback(Node node, Node target, int? maxDepth = null)
        {
            node.AddFallback(target, maxDepth);
            return node;
        }
        public bool RemoveFallback(string key, string targetKey, int? maxDepth = null, bool matchMaxDepth = false)
        {
            return RemoveFallback(NodeAt(key), GetNodeFromIdent(targetKey), maxDepth, matchMaxDepth);
        }
        public bool RemoveFallback(Node node, Node target, int? maxDepth = null, bool matchMaxDepth = false)
        {
            return node.RemoveFallback(target, maxDepth, matchMaxDepth);
        }
        public T? SetValue(uint nodeID, T? value, ulong netSender = 0)
        {
            if (!nodesByID.ContainsKey(nodeID))
            {
                Debug.Log($"Tried to set unknown nodeID '{nodeID}' in '{treeID} ({identifier}) to '{value}' via netSender {netSender} ");
                return default(T);
            }
            //throw new KeyNotFoundException(nameof(nodeID));
            var node = nodesByID[nodeID];
            Debug.Log($"Setting value of node by ID '{nodeID}' ({node.GetNodeTreeString()}) in tree {treeID} ({identifier}) to '{value}' (netSender: {netSender})");
            return SetValue(nodesByID[nodeID].nodeIdentity, value, netSender);
        }
        public T? ResetToDefault(string key, ulong netSender = 0)
        {
            if (!nodes.ContainsKey(key))
            {
                Debug.Log($"Tried to reset unknown key '{key}' in '{treeID} ({identifier}) to default via netSender {netSender} ");
                return default(T);
            }
            Node node = nodes[key];
            return SetValue(key, node.DefaultValue, netSender);
        }
        public T? SetValue(string key, T? value, ulong netSender = 0)
        {
            if (!nodes.ContainsKey(key))
            {
                Debug.Log($"Tried to set unknown key '{key}' in '{treeID} ({identifier}) to '{value}' via netSender {netSender} ");
                return default(T);
            }
            //throw new KeyNotFoundException(nameof(key));
            Node node = nodes[key];
            Debug.Log($"Setting value of node by key '{node.GetNodeTreeString()}' ({node.nodeID}) in tree {treeID} ({identifier}) to '{value}' (netSender: {netSender})");
            node.SetValue(value);
            //if (netSender == 0) // We need to sync these values between clients.
            //{
            //    Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            //    switch (Type.GetTypeCode(type))
            //    {
            //        case TypeCode.Boolean:
            //            {
            //                pStructs.pBoolOverideTreeInfo info = new pStructs.pBoolOverideTreeInfo();
            //                info.treeID = treeID;
            //                info.keyId = node.nodeID;
            //                if (value is null)
            //                {
            //                    info.value = false;
            //                    info.isNull = true;
            //                }
            //                else if (value is bool v)
            //                {
            //                    info.value = v;
            //                    info.isNull = false;
            //                }
            //                else
            //                {
            //                    throw new InvalidCastException($"Expected bool value for key '{key}', but got {value.GetType().Name}.");
            //                }
            //                NetworkAPI.InvokeEvent<pStructs.pBoolOverideTreeInfo>("SetBoolOverideTree", info);
            //                break;
            //            }
            //        case TypeCode.Int32:
            //            {
            //                pStructs.pIntOverideTreeInfo info = new pStructs.pIntOverideTreeInfo();
            //                info.treeID = treeID;
            //                info.keyId = node.nodeID;
            //                if (value is null)
            //                {
            //                    info.value = 0;
            //                    info.isNull = true;
            //                }
            //                else if (value is int v)
            //                {
            //                    info.value = v;
            //                    info.isNull = false;
            //                }
            //                else
            //                {
            //                    throw new InvalidCastException($"Expected int value for key '{key}', but got {value.GetType().Name}.");
            //                }
            //                NetworkAPI.InvokeEvent<pStructs.pIntOverideTreeInfo>("SetIntOverideTree", info);
            //                break;
            //            }
            //        case TypeCode.Single:
            //            {
            //                pStructs.pFloatOverideTreeInfo info = new pStructs.pFloatOverideTreeInfo();
            //                info.treeID = treeID;
            //                info.keyId = node.nodeID;
            //                if (value is null)
            //                {
            //                    info.value = 0f;
            //                    info.isNull = true;
            //                }
            //                else if (value is float v)
            //                {
            //                    info.value = v;
            //                    info.isNull = false;
            //                }
            //                else
            //                {
            //                    throw new InvalidCastException($"Expected float value for key '{key}', but got {value.GetType().Name}.");
            //                }
            //                NetworkAPI.InvokeEvent<pStructs.pFloatOverideTreeInfo>("SetFloatOverideTree", info);
            //                break;
            //            }
            //        default:
            //            {
            //                Debug.Log($"set unusual type ({value?.GetType().Name ?? "null"}) in override tree.");
            //                break;
            //            }

            //    }
            //}
            //node.menuNode?.UpdateNode();
            OnValueSet?.Invoke(this, node.nodeID, key, value, netSender);
            return ValueAt(key);
        }
        public T? GetValue()
        {
            return rootNode.GetValue();
        }
        public T? GetValue(string key = null)
        {
            if (key == null)
                return rootNode.GetValue();
            return NodeAt(key).GetValue();
        }
        public T? ValueAt(string key, bool shouldthrow = true)
        {
            return NodeAt(key, shouldthrow).ValueAt();
        }
        public bool HasParrent(string key)
        {
            Node node = NodeAt(key);
            return node.HasParrent();
        }
        public bool HasParrentAndValue(string key)
        {
            return HasParrent(key) && HasValue(key);
        }
        public bool HasKey(string key)
        {
            return nodes.ContainsKey(key);
        }
        public bool HasValue(string key)
        {
            if (HasKey(key))
                return GetNodeFromIdent(key).HasValue();
            return false;
        }
        public bool MatchingDefaultValue(string key)
        {
            Node node = NodeAt(key);
            return node.MatchingDefaultValue();
        }
        public bool IsDefaultValue(string key)
        {
            Node node = NodeAt(key);
            return node.IsDefaultValue();
        }
        public bool HasDefault(string key)
        {
            Node node = NodeAt(key);
            return node.HasDefault();
        }
        public void SetDefaultValue(string key, T? defaultValue)
        {
            NodeAt(key).SetDefaultValue(defaultValue);
        }
        public void SetHasDefaultValue(string key, bool hasDefaultValue)
        {
            NodeAt(key).SetHasDefaultValue(hasDefaultValue);
        }
        private Node NodeAt(string key, bool shouldThrow = true)
        {
            if (!nodes.ContainsKey(key))
                if (shouldThrow)
                    throw new KeyNotFoundException(nameof(key));
                else
                    return default;
            return nodes[key];
        }
        public bool AllDefault(IEnumerable<string> keys)
        {
            foreach (string key in keys)
                if (!IsDefaultValue(key))
                    return false;
            return true;
        }
        public T GetDefaultValue(string key)
        {
            return NodeAt(key).DefaultValue;
        }
        public object IGetDefaultValue(string key)
        {
            return (object)GetDefaultValue(key);
        }

        public object IValueAt(string key)
        {
            return (object)ValueAt(key);
        }

        public object IGetValue(string key)
        {
            return (object)GetValue(key);
        }

        public bool IHasValue(string key)
        {
            return HasValue(key);
        }

        public bool IHasKey(string key)
        {
            return HasKey(key);
        }

        public bool IMatchingDefaultValue(string key)
        {
            return MatchingDefaultValue(key);
        }

        public bool IHasParrent(string key)
        {
            return HasParrent(key);
        }

        public bool IHasDefault(string key)
        {
            return HasDefault(key);
        }
    }
}
