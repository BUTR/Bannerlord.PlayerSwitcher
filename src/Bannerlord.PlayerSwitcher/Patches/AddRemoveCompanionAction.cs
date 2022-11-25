using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.CampaignSystem;

namespace Bannerlord.PlayerSwitcher.Patches
{
    /// <summary>
    /// Disables old hero party destruction when switching until disposed
    /// </summary>
    public class AddRemoveCompanionActionHandler : IDisposable
    {
        public AddRemoveCompanionActionHandler() => AddRemoveCompanionActionPatch.SkipChange = true;
        public void Dispose() => AddRemoveCompanionActionPatch.SkipChange = false;
    }

    internal static class AddRemoveCompanionActionPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            return true &
                harmony.TryPatch(
                    original: AccessTools2.Method(typeof(Clan), "OnHeroAdded"),
                    prefix: AccessTools2.Method(typeof(AddRemoveCompanionActionPatch), nameof(Prefix))) &
                harmony.TryPatch(
                    original: AccessTools2.Method(typeof(Clan), "OnHeroRemoved"),
                    prefix: AccessTools2.Method(typeof(AddRemoveCompanionActionPatch), nameof(Prefix)));
        }

        private static bool Prefix() => !SkipChange;
    }
}