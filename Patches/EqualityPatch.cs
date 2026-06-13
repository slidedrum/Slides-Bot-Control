using HarmonyLib;
using Player;
using System.Linq;
using System.Reflection;

namespace BotControl.Patches
{
    [HarmonyPatch]
    public static class EqualityPatchs
    {
        public static void test()
        {
            foreach (var m in typeof(Il2CppSystem.Object)
                .GetMethods())
            {
                ZiMain.log.LogInfo(m.Name);
            }
        }
        //static MethodBase TargetMethod()
        //{
        //    return typeof(Il2CppSystem.Object)
        //        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        //        .Single(m =>
        //            m.Name == "op_Equality" &&
        //            m.GetParameters().Length == 2);
        //}

        //[HarmonyPrefix]
        //public static bool PreEquality(object First, object Second, ref bool __result)
        //{
        //    if (First is null || Second is null)
        //    {
        //        __result = ReferenceEquals(First, Second);
        //        return false;
        //    }

        //    if (First.TryCastTo(out PlayerBotActionBase.Descriptor a) &&
        //        Second.TryCastTo(out PlayerBotActionBase.Descriptor b))
        //    {
        //        __result = a.Pointer == b.Pointer;
        //        return false;
        //    }

        //    return true;
        //}
    }
    //[HarmonyPatch]
    //public static class InequalityPatchs
    //{
    //    static MethodBase TargetMethod()
    //    {
    //        return typeof(Il2CppSystem.Object)
    //            .GetMethods(BindingFlags.Public | BindingFlags.Static)
    //            .Single(m =>
    //                m.Name == "op_Inequality" &&
    //                m.GetParameters().Length == 2);
    //    }

    //    [HarmonyPrefix]
    //    public static bool PreInequality(object First, object Second, ref bool __result)
    //    {
    //        if (First is null || Second is null)
    //        {
    //            __result = !ReferenceEquals(First, Second);
    //            return false;
    //        }

    //        if (First.TryCastTo(out PlayerBotActionBase.Descriptor a) &&
    //            Second.TryCastTo(out PlayerBotActionBase.Descriptor b))
    //        {
    //            __result = a.Pointer != b.Pointer;
    //            return false;
    //        }

    //        return true;
    //    }
    //}
}
