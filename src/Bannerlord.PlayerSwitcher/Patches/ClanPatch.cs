using Bannerlord.PlayerSwitcher.CampaignBehaviors;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.CampaignSystem;

namespace Bannerlord.PlayerSwitcher.Patches
{
    public static class ClanPatch
    {
        private delegate Clan GetPlayerDefaultFactionDelegate(Campaign instance);
        private static readonly GetPlayerDefaultFactionDelegate? GetPlayerDefaultFaction =
            AccessTools2.GetPropertyGetterDelegate<GetPlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        public static bool Enable(Harmony harmony)
        {
            return harmony.TryPatch(
                original: AccessTools2.PropertyGetter(typeof(Clan), "PlayerClan"),
                prefix: AccessTools2.Method(typeof(ClanPatch), nameof(GetPlayerClanPrefix)));
        }

        private static bool GetPlayerClanPrefix(ref Clan? __result)
        {
            var selectedClan = StorageCampaignBehavior.Instance?.SelectedClan;
            __result = selectedClan ?? GetPlayerDefaultFaction?.Invoke(Campaign.Current);
            return false;
        }
    }
}