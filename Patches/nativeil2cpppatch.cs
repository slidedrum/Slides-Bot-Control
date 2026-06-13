//using HarmonyLib;
//using Il2CppInterop.Runtime;
//using Il2CppInterop.Runtime.Injection;
//using Il2CppInterop.Runtime.Runtime;
//using MonoMod.RuntimeDetour;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Text;

//// --- IMPORTANT NOTE IF YOU ARE EDITING THIS FILE ---
//// --- COMMENT OUT THIS ENTIRE FILE AND USE MY CUSTOM IL2CPP BUILD ---
//// --- IF YOU DON'T, THE GAME WILL CRASH WITH UNITY EXPLORER ---
////
//// Il2CppInterop's InjectorHelpers / ClassInjector methods are managed CLR code, not
//// native IL2CPP exports. INativeDetour (Dobby) is only safe for game IL2CPP pointers
//// (see Patches/Native/*). These hooks use MonoMod.RuntimeDetour instead.

//namespace BotControl;

//public static unsafe class Il2CppInteropNativePatches
//{
//    private static Hook GetIl2CppTypeFullNameHook;
//    private static Hook AddTypeToLookupHook;

//    internal static void ApplyNativePatch()
//    {
//        ApplyGetIl2CppTypeFullNamePatch();
//        ApplyAddTypeToLookupPatch();
//    }

//    private static void ApplyGetIl2CppTypeFullNamePatch()
//    {
//        if (GetIl2CppTypeFullNameHook != null)
//        {
//            return;
//        }

//        MethodInfo target = AccessTools.Method(typeof(ClassInjector), "GetIl2CppTypeFullName");
//        MethodInfo replacement = typeof(Il2CppInteropNativePatches).GetMethod(
//            nameof(GetIl2CppTypeFullNamePatch),
//            BindingFlags.Static | BindingFlags.NonPublic);

//        GetIl2CppTypeFullNameHook = new Hook(target, replacement);
//    }

//    private static void ApplyAddTypeToLookupPatch()
//    {
//        if (AddTypeToLookupHook != null)
//        {
//            return;
//        }

//        MethodInfo target = AccessTools.Method(
//            AccessTools.TypeByName("Il2CppInterop.Runtime.Injection.InjectorHelpers"),
//            "AddTypeToLookup",
//            new[] { typeof(Type), typeof(IntPtr) });

//        MethodInfo replacement = typeof(Il2CppInteropNativePatches).GetMethod(
//            nameof(AddTypeToLookupPatch),
//            BindingFlags.Static | BindingFlags.NonPublic);

//        AddTypeToLookupHook = new Hook(target, replacement);
//    }

//    private static string GetIl2CppTypeFullNamePatch(Il2CppTypeStruct* typePointer)
//    {
//        var klass = UnityVersionHandler.Wrap((Il2CppClass*)IL2CPP.il2cpp_class_from_type((IntPtr)typePointer));
//        var assembly = UnityVersionHandler.Wrap(UnityVersionHandler.Wrap(klass.Image).Assembly);
//        var fullName = new StringBuilder();
//        var names = new Stack<string>();
//        var declaringType = klass;
//        var outerType = klass;
//        do
//        {
//            names.Push(Marshal.PtrToStringUTF8(declaringType.Name) ?? "");
//            outerType = declaringType;
//        }
//        while ((declaringType = UnityVersionHandler.Wrap(declaringType.DeclaringType)) != default);
//        var namespaceName = outerType.Namespace != IntPtr.Zero ? Marshal.PtrToStringUTF8(outerType.Namespace) ?? "" : "";

//        fullName.Append(namespaceName);
//        if (namespaceName.Length > 0)
//        {
//            fullName.Append('.');
//        }

//        fullName.Append(string.Join("+", names));

//        var assemblyName = Marshal.PtrToStringUTF8(assembly.Name.Name);
//        if (assemblyName != "mscorlib")
//        {
//            fullName.Append(", ");
//            fullName.Append(assemblyName);
//        }

//        return fullName.ToString();
//    }

//    [Flags]
//    public enum Il2CppTypeNameOptions
//    {
//        None = 0,
//        Namespace = 1 << 0,
//        Name = 1 << 1,
//        Assembly = 1 << 2,
//        All = Namespace | Name | Assembly
//    }

//    internal static string GetTypeName(Type type, Il2CppTypeNameOptions options = Il2CppTypeNameOptions.All)
//    {
//        var assembly = type.Assembly;
//        var fullName = new StringBuilder();
//        var names = new Stack<string>();
//        var outerType = type;
//        while (outerType.DeclaringType != null)
//        {
//            outerType = outerType.DeclaringType;
//        }

//        var namespaceName = outerType.Namespace ?? "";
//        if (options.HasFlag(Il2CppTypeNameOptions.Namespace) && namespaceName.Length > 0)
//        {
//            fullName.Append(namespaceName);
//            fullName.Append('.');
//        }

//        if (options.HasFlag(Il2CppTypeNameOptions.Name) && names.Count > 0)
//        {
//            fullName.Append(string.Join("+", names));
//        }

//        if (options.HasFlag(Il2CppTypeNameOptions.Assembly))
//        {
//            var assemblyName = assembly.FullName;
//            if (assemblyName != "mscorlib")
//            {
//                fullName.Append(", ");
//                fullName.Append(assemblyName);
//            }
//        }

//        return fullName.ToString();
//    }

//    private static void AddTypeToLookupPatch(Type type, IntPtr typePointer)
//    {
//        var injectorHelpers = AccessTools.TypeByName("Il2CppInterop.Runtime.Injection.InjectorHelpers");
//        var s_ClassNameLookup = AccessTools.Field(injectorHelpers, "s_ClassNameLookup");

//        string klass = GetTypeName(type, Il2CppTypeNameOptions.Name);
//        if (klass.Length == 0)
//        {
//            return;
//        }

//        string namespaze = GetTypeName(type, Il2CppTypeNameOptions.Namespace);

//        var attr = Attribute.GetCustomAttribute(
//            type,
//            AccessTools.TypeByName("Il2CppInterop.Runtime.Attributes.ClassInjectionAssemblyTargetAttribute"));

//        IEnumerable<IntPtr> images;
//        if (attr == null)
//        {
//            var il2cppType = AccessTools.TypeByName("Il2CppInterop.Runtime.IL2CPP");
//            var getIl2CppImages = AccessTools.Method(il2cppType, "GetIl2CppImages");
//            images = (IEnumerable<IntPtr>)getIl2CppImages.Invoke(null, null)!;
//        }
//        else
//        {
//            var getImagePointers = AccessTools.Method(attr.GetType(), "GetImagePointers");
//            images = (IEnumerable<IntPtr>)getImagePointers.Invoke(attr, null)!;
//        }

//        var dict = (IDictionary<(string, string, IntPtr), IntPtr>)s_ClassNameLookup.GetValue(null)!;
//        foreach (var image in images)
//        {
//            dict.Add((namespaze, klass, image), typePointer);
//        }
//    }
//}
