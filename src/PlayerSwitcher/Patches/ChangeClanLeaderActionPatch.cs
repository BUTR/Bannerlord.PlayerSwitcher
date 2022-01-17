using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace PlayerSwitcher
{
    public class ChangeClanLeaderActionPatch
    {
        private delegate void TransferOldPlayersEquipmentToNewPlayerDelegate(Hero oldPlayer, Hero newPlayer);
        private static readonly TransferOldPlayersEquipmentToNewPlayerDelegate? TransferOldPlayersEquipmentToNewPlayer =
            AccessTools2.GetDelegate<TransferOldPlayersEquipmentToNewPlayerDelegate>(typeof(ChangeClanLeaderAction), "TransferOldPlayersEquipmentToNewPlayer");

        private delegate CampaignEventDispatcher GetCampaignEventDispatcherDelegate(Campaign instance);
        private static readonly GetCampaignEventDispatcherDelegate? GetCampaignEventDispatcher =
            AccessTools2.GetPropertyGetterDelegate<GetCampaignEventDispatcherDelegate>(typeof(Campaign), "CampaignEventDispatcher");

        public static bool Enable(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools2.Method(typeof(ChangeClanLeaderAction), "ApplyInternal"),
                prefix: new HarmonyMethod(typeof(ChangeClanLeaderActionPatch).GetMethod(nameof(ApplyInternalPrefix))));

            return true;
        }

        private static bool ApplyInternalPrefix(Clan clan, Hero? newLeader = null)
        {
            Hero leader;
            if (clan.StringId == "neutral")
            {
                clan = new Clan();
                var name = new TextObject($"Posse of {newLeader.Name}");
                clan.InitializeClan(name, name, newLeader.Culture, Banner.CreateRandomClanBanner());
                leader = newLeader;
                leader.Clan = clan;
            }
            else
            {
                leader = clan.Leader;
            }

            var noChange = leader == newLeader;
            if (newLeader is null)
            {
                var heirApparents = leader.Clan.GetHeirApparents();
                if (heirApparents.Count == 0)
                {
                    return false;
                }
                var highestPoint = (from h in heirApparents
                    orderby h.Value descending
                    select h).FirstOrDefault().Value;
                newLeader = (from h in heirApparents
                    where h.Value.Equals(highestPoint)
                    select h).GetRandomElementInefficiently().Key;
            }
            GiveGoldAction.ApplyBetweenCharacters(leader, newLeader, leader.Gold, true);
            if (newLeader.GovernorOf is not null)
            {
                ChangeGovernorAction.Apply(newLeader.GovernorOf, null);
            }
            if (!newLeader.IsPrisoner && !newLeader.IsFugitive && !newLeader.IsReleased)
            {
                var mobileParty = newLeader.PartyBelongedTo ?? clan.CreateNewMobileParty(newLeader);
                if (mobileParty.LeaderHero != newLeader)
                {
#if e160 || e161 || e162 || e163 || e164 || e165
                    mobileParty.ChangePartyLeader(newLeader.CharacterObject);
#elif e170 || e171
                    mobileParty.ChangePartyLeader(newLeader);
#else
#error NOT SET
#endif
                }
                if (leader == Hero.MainHero)
                {
                    TransferOldPlayersEquipmentToNewPlayer?.Invoke(leader, newLeader);
                }
            }
            foreach (var hero in Hero.AllAliveHeroes)
            {
                var relationChangeAfterClanLeaderIsDead = Campaign.Current.Models.DiplomacyModel.GetRelationChangeAfterClanLeaderIsDead(leader, hero);
                var heroRelation = CharacterRelationManager.GetHeroRelation(newLeader, hero);
                newLeader.SetPersonalRelation(hero, heroRelation + relationChangeAfterClanLeaderIsDead);
            }
            if (!noChange) leader.Clan.SetLeader(newLeader);
            GetCampaignEventDispatcher?.Invoke(Campaign.Current).OnClanLeaderChanged(leader, newLeader);
            return false;
        }
    }
}