using BepInEx.Unity.IL2CPP.Hook;
using ChainedPuzzles;
using GTFO.API;
using Il2CppInterop.Runtime.Runtime;
using Player;
using System;

namespace BotControl.Patches // This is an AI generated patch, I had an AI port the C decomp and create this patch.  I may re-work things when I understand native patches.
{ 
    public static class UseBioscanEvaluatePatches // This patch was to debug bots not caring about scan circles, it's intended to let me set breakpoints and step through the method
                                                  // I am 99% sure that this method is the reason the bots don't care about scanners sometimes.
                                                  // For some unknown reason this returns false, causing the action to fail and be removed, thus they don't care anymore.
    {
        private static INativeDetour? EvaluateDetour;

        private unsafe delegate bool d_Evaluate(
            IntPtr bioscan,
            IntPtr bot,
            float* standRadius,
            int* nrOthers,
            Il2CppMethodInfo* methodInfo);

        internal unsafe static void ApplyNativePatch()
        {
            if (EvaluateDetour != null)
            {
                return;
            }

            EvaluateDetour = INativeDetour.CreateAndApply(
                (nint)Il2CppAPI.GetIl2CppMethod<PlayerBotActionUseBioscan.Descriptor>(
                    nameof(PlayerBotActionUseBioscan.Descriptor.Evaluate),
                    typeof(bool).Name,
                    true,
                    new[]
                    {
                        typeof(CP_Bioscan_Core).Name,
                        typeof(PlayerAIBot).Name,
                        typeof(float).MakeByRefType().Name,
                        typeof(int).MakeByRefType().Name,
                    }),
                EvaluatePatch,
                out d_Evaluate _);
        }

        private unsafe static bool EvaluatePatch(
            IntPtr bioscan,
            IntPtr bot,
            float* standRadius,
            int* nrOthers,
            Il2CppMethodInfo* methodInfo)
        {
            _ = methodInfo;

            CP_Bioscan_Core bioscanObj = new(bioscan);
            PlayerAIBot botObj = new(bot);

            bool result = EvaluateManaged(bioscanObj, botObj, out float standRadiusLocal, out int nrOthersLocal);

            if (standRadius != null)
            {
                *standRadius = standRadiusLocal;
            }

            if (nrOthers != null)
            {
                *nrOthers = nrOthersLocal;
            }

            return result;
        }
        internal static bool EvaluateManaged(
            CP_Bioscan_Core bioscan,
            PlayerAIBot bot,
            out float standRadius,
            out int nrOthers)
        {
            standRadius = 0f;
            nrOthers = 0;
            pBioscanState state = bioscan.State;
            if (state.status != eBioscanStatus.Waiting && state.status != eBioscanStatus.Scanning)
            {
                return false;
            }
            CP_PlayerScanner cp_PlayerScanner = bioscan.PlayerScanner.Cast<CP_PlayerScanner>();
            if (cp_PlayerScanner == null)
            {
                return false;
            }
            standRadius = cp_PlayerScanner.Radius;
            switch (cp_PlayerScanner.ScanPlayersRequired)
            {
                case PlayerRequirement.None:
                    if (cp_PlayerScanner.LastPlayerCount != 0)
                    {
                        int num = 0;
                        for (int i = 0; i < bioscan.PlayersOnScan.Count; i++)
                        {
                            if (bioscan.PlayersOnScan[i] != null && !bioscan.PlayersOnScan[i].Owner.IsBot)
                            {
                                num++;
                            }
                        }
                        nrOthers = num;
                    }
                    nrOthers += PlayerManager.Current.CountObjectReservations(bot.Agent.CharacterID, bioscan.gameObject);
                    if (!cp_PlayerScanner.CanGoFaster(nrOthers))
                    {
                        return false;
                    }
                    if (bioscan.HasAlarm)
                    {
                        return true;
                    }
                    int num2 = bioscan.Owner.NRofPuzzles();
                    for (int j = 0; j < num2; j++)
                    {
                        CP_Bioscan_Core puzzle = bioscan.Owner.GetPuzzle(j).Cast<CP_Bioscan_Core>();
                        if (puzzle != null && puzzle != bioscan)
                        {
                            CP_Bioscan_Core cp_Bioscan_Core = puzzle;
                            if (cp_Bioscan_Core != null)
                            {
                                CP_PlayerScanner cp_PlayerScanner2 = cp_Bioscan_Core.PlayerScanner.Cast<CP_PlayerScanner>();
                                if (cp_PlayerScanner2 != null && cp_PlayerScanner2.LastPlayerCount != 0)
                                {
                                    for (int k = 0; k < cp_Bioscan_Core.PlayersOnScan.Count; k++)
                                    {
                                        if (cp_Bioscan_Core.PlayersOnScan[k] != null && !cp_Bioscan_Core.PlayersOnScan[k].Owner.IsBot)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case PlayerRequirement.All:
                    if (bioscan.HasAlarm)
                    {
                        return true;
                    }
                    for (int l = 0; l < bioscan.PlayersOnScan.Count; l++)
                    {
                        if (bioscan.PlayersOnScan[l] != null && !bioscan.PlayersOnScan[l].Owner.IsBot)
                        {
                            return true;
                        }
                    }
                    return false;
                case PlayerRequirement.Solo:
                    return false;
            }
            return false;
        }
    }
}
