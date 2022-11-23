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
            return true &
                harmony.TryPatch(
                    original: AccessTools2.Method($"TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.HeirSelectionCampaignBehavior:OnBeforePlay1erCharacterChanged"),
                    prefix: AccessTools2.Method(typeof(HeirSelectionCampaignBehaviorPatch), nameof(Prefix))) &
                harmony.TryPatch(
                    original: AccessTools2.Method($"TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.HeirSelectionCampaignBehavior:OnPlayerCharacterChanged"),
                    prefix: AccessTools2.Method(typeof(HeirSelectionCampaignBehaviorPatch), nameof(Prefix)));
        }

        private static bool Prefix() => !SkipChange;
    }
}