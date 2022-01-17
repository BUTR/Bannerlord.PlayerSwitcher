using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.CampaignSystem;

namespace PlayerSwitcher
{
    public static class ClanPatch
    {
        private delegate Clan GetPlayerDefaultFactionDelegate(Campaign instance);
        private static readonly GetPlayerDefaultFactionDelegate? GetPlayerDefaultFaction =
            AccessTools2.GetPropertyGetterDelegate<GetPlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        public static bool Enable(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools2.PropertyGetter(typeof(Clan), "PlayerClan"),
                prefix: new HarmonyMethod(AccessTools2.Method(typeof(ClanPatch), nameof(GetPlayerClanPrefix))));

            return true;
        }

        private static bool GetPlayerClanPrefix(ref Clan? __result)
        {
            var selectedClan = SyncBehavior.Instance?.Clan;
            __result = selectedClan ?? GetPlayerDefaultFaction?.Invoke(Campaign.Current);
            return false;
        }
    }
}