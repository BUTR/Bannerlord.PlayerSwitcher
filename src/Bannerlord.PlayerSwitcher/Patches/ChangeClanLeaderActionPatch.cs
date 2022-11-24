using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace Bannerlord.PlayerSwitcher.Patches
{
    internal class ChangeClanLeaderActionPatch
    {
        public static bool Enable(Harmony harmony)
        {
            return harmony.TryPatch(
                original: AccessTools2.Method(typeof(ChangeClanLeaderAction), "ApplyInternal"),
                prefix: AccessTools2.Method(typeof(ChangeClanLeaderActionPatch), nameof(ApplyInternalPrefix)));
        }

        private static bool ApplyInternalPrefix(ref Clan clan, Hero? newLeader = null)
        {
            // Create a random clan for neutrals
            if (newLeader is not null && clan.StringId == "neutral")
            {
                clan = new Clan();
                var name = Strings.PosseOfText.SetTextVariable("LEADER", newLeader.Name);
                clan.InitializeClan(name, name, newLeader.Culture, Banner.CreateRandomClanBanner());
            }

            return true;
        }
    }
}