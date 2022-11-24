using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using SandBox.CampaignBehaviors;

using System;

namespace Bannerlord.PlayerSwitcher.Patches
{
    /// <summary>
    /// Disables Heir selection until disposed
    /// </summary>
    public class ChangePlayerCharacterActionHandler : IDisposable
    {
        public ChangePlayerCharacterActionHandler() => HeirSelectionCampaignBehaviorPatch.SkipChange = true;
        public void Dispose() => HeirSelectionCampaignBehaviorPatch.SkipChange = false;
    }

    internal class HeirSelectionCampaignBehaviorPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            return true &
                harmony.TryPatch(
                    original: AccessTools2.Method(typeof(HeirSelectionCampaignBehavior), "OnBeforePlayerCharacterChanged"),
                    prefix: AccessTools2.Method(typeof(HeirSelectionCampaignBehaviorPatch), nameof(Prefix))) &
                harmony.TryPatch(
                    original: AccessTools2.Method(typeof(HeirSelectionCampaignBehavior), "OnPlayerCharacterChanged"),
                    prefix: AccessTools2.Method(typeof(HeirSelectionCampaignBehaviorPatch), nameof(Prefix)));
        }

        private static bool Prefix() => !SkipChange;
    }
}