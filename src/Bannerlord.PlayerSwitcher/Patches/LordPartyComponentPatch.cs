using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.CampaignSystem.Party.PartyComponents;

namespace Bannerlord.PlayerSwitcher.Patches
{
    /// <summary>
    /// Disables old party owner switch until disposed
    /// </summary>
    public class LordPartyComponentHandler : IDisposable
    {
        public LordPartyComponentHandler() => LordPartyComponentPatch.SkipChange = true;
        public void Dispose() => LordPartyComponentPatch.SkipChange = false;
    }

    internal class LordPartyComponentPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            return true &
                harmony.TryPatch(
                    original: AccessTools2.Method(typeof(LordPartyComponent), "ChangePartyOwner"),
                    prefix: AccessTools2.Method(typeof(LordPartyComponentPatch), nameof(Prefix)));
        }

        private static bool Prefix() => !SkipChange;
    }
}