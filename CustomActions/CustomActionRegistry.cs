using BotControl.CustomActions;
using Il2CppInterop.Runtime.Injection;
using Player;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class CustomActionRegistry
{
    private static readonly (Type ActionType, Func<PlayerAIBot, CustomActionBase.Descriptor> CreateDescriptor)[] Actions;

    static CustomActionRegistry()
    {
        Actions = typeof(CustomActionBase).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(CustomActionBase).IsAssignableFrom(t))
            .Select(t => (Action: t, Descriptor: t.GetNestedType("Descriptor", BindingFlags.Public | BindingFlags.NonPublic)))
            .Where(pair => pair.Descriptor != null
                && typeof(CustomActionBase.Descriptor).IsAssignableFrom(pair.Descriptor)
                && !pair.Descriptor.IsAbstract)
            .Select(pair => (pair.Action, CreateDescriptorFactory(pair.Descriptor)))
            .ToArray();
    }

    private static Func<PlayerAIBot, CustomActionBase.Descriptor> CreateDescriptorFactory(Type descriptorType)
    {
        var ctor = descriptorType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(PlayerAIBot) },
            modifiers: null);

        if (ctor == null)
            throw new InvalidOperationException($"No (PlayerAIBot) constructor on {descriptorType.FullName}");

        var botParam = Expression.Parameter(typeof(PlayerAIBot), "bot");
        var newExpr = Expression.New(ctor, botParam);
        var lambda = Expression.Lambda<Func<PlayerAIBot, CustomActionBase.Descriptor>>(newExpr, botParam);
        return lambda.Compile();
    }

    public static void RegisterIl2CppTypes()
    {
        ClassInjector.RegisterTypeInIl2Cpp(typeof(CustomActionBase));
        ClassInjector.RegisterTypeInIl2Cpp(typeof(CustomActionBase.Descriptor));

        foreach (var (actionType, _) in Actions)
        {
            ClassInjector.RegisterTypeInIl2Cpp(actionType);
            ClassInjector.RegisterTypeInIl2Cpp(actionType.GetNestedType("Descriptor", BindingFlags.Public | BindingFlags.NonPublic)!);
        }
    }

    public static void Setup(PlayerAIBot bot)
    {
        if (bot == null)
            throw new ArgumentNullException(nameof(bot));

        var data = zActions.GetOrCreateData(bot);

        foreach (var (_, createDescriptor) in Actions)
        {
            var descriptor = createDescriptor(bot);

            // Safety net — vanilla ports assign Bot explicitly in ctor anyway
            if (descriptor.Bot == null)
                descriptor.Bot = bot;

            var action = (CustomActionBase)descriptor.CreateAction();

            data.customActionDescriptors.Add(descriptor);
            data.customActionBases.Add(action);
        }
    }
}