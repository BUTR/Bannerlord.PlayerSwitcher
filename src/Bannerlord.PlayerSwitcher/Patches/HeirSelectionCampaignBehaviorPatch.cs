using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.PlayerSwitcher.Patches
{
    public class ChangePlayerCharacterActionHandler : IDisposable
    {
        public ChangePlayerCharacterActionHandler()
        {
            HeirSelectionCampaignBehaviorPatch.SkipChange = true;
        }

        public void Dispose()
        {
            HeirSelectionCampaignBehaviorPatch.SkipChange = false;
        }
    }

    public class HeirSelectionCampaignBehaviorPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            const string @base = "TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.HeirSelectionCampaignBehavior";
            return true &
                harmony.TryPatch(
                    original: AccessTools2.Method($"{@base}:OnBeforePlayerCharacterChanged"),
                    prefix: AccessTools2.Method(typeof(HeirSelectionCampaignBehaviorPatch), nameof(Prefix))) &
                harmony.TryPatch(
                    original: AccessTools2.Method($"{@base}:OnPlayerCharacterChanged"),
                    prefix: AccessTools2.Method(typeof(HeirSelectionCampaignBehaviorPatch), nameof(Prefix)));
        }

        private static bool Prefix() => !SkipChange;
    }
}