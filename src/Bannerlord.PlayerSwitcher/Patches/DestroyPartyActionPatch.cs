using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.CampaignSystem.Actions;

namespace Bannerlord.PlayerSwitcher.Patches
{
    /// <summary>
    /// Disables old hero party destruction when switching until disposed
    /// </summary>
    public class DestroyPartyActionHandler : IDisposable
    {
        public DestroyPartyActionHandler() => DestroyPartyActionPatch.SkipChange = true;
        public void Dispose() => DestroyPartyActionPatch.SkipChange = false;
    }

    internal static class DestroyPartyActionPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            return true &
                harmony.TryPatch(
                    original: AccessTools2.Method(typeof(DestroyPartyAction), "Apply"),
                    prefix: AccessTools2.Method(typeof(LordPartyComponentPatch), nameof(Prefix)));
        }

        private static bool Prefix() => !SkipChange;
    }
}