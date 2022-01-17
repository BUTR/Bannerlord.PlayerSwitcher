using Bannerlord.BUTR.Shared.Extensions;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using Helpers;

using PlayerSwitcher.HotKeys;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PlayerSwitcher
{
    public sealed class SubModule : MBSubModuleBase
    {
        private delegate CampaignEventDispatcher GetCampaignEventDispatcherDelegate(Campaign instance);
        private static readonly GetCampaignEventDispatcherDelegate? GetCampaignEventDispatcher =
            AccessTools2.GetPropertyGetterDelegate<GetCampaignEventDispatcherDelegate> (typeof(Campaign), "CampaignEventDispatcher");

        private readonly Harmony _harmony = new("Bannerlord.PlayerSwitcher");

        protected override void OnSubModuleLoad()
        {
            ClanPatch.Enable(_harmony);
            ChangeClanLeaderActionPatch.Enable(_harmony);
            SPInventoryVMPatch.Enable(_harmony);
            
            base.OnSubModuleLoad();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (Bannerlord.ButterLib.HotKeys.HotKeyManager.Create("PlayerSwitcher") is { } hkm)
            {
                hkm.Add<SwitchKey>();
                hkm.Build();
            }

            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter cgs)
            {
                cgs.AddBehavior(new SyncBehavior());
            }
        }

        private static IEnumerable<Clan> HeirApparents()
        {
            foreach (var clan in Clan.All)
            {
                if (clan is null)
                    continue;

                if (clan.Heroes.Count == 0)
                    continue;

                if (clan == Clan.PlayerClan && clan.Heroes.Count == 1)
                    continue;

                yield return clan;
            }
        }

        internal static void SelectClan()
        {
            static IEnumerable<InquiryElement> ClanInquiries()
            {
                foreach (var clan in HeirApparents().OrderBy(x => x.Tier))
                {
                    if (clan.StringId == "neutral")
                        continue;

                    yield return new InquiryElement(clan, clan.Name.ToString(), new ImageIdentifier(clan.Banner));
                }
            }

            InformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("Player Switcher").ToString(),
                    new TextObject("Select a clan to choose from their heroes.").ToString(),
                    ClanInquiries().ToList(),
                    true,
                    1,
                    new TextObject("Done").ToString(),
                    "",
                    OnFactionSelectionOver,
                    null)
            );

            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
        }

        private static void OnFactionSelectionOver(List<InquiryElement> element)
        {
            static IEnumerable<InquiryElement> ClanInquiries(Clan clan)
            {
                foreach (var hero in GetHeroes(clan))
                {
                    var parent = new TextObject("{HERO.NAME}");
                    StringHelpers.SetCharacterProperties("HERO", hero.CharacterObject, parent);
                    yield return new InquiryElement(hero, parent.ToString(), new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject)));
                }
            }

            if (element.First().Identifier is not Clan clan)
                return;

            InformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("Player Switcher").ToString(),
                    new TextObject("Select a hero to play as.").ToString(),
                    ClanInquiries(clan).ToList(),
                    true,
                    1,
                    new TextObject("DONE").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );
        }

        private static IEnumerable<Hero> GetHeroes(Clan clan)
        {
            foreach (var hero in clan.Heroes)
            {
                if (hero is null)
                    continue;

                if (hero == Hero.MainHero)
                    continue;

                if (!hero.IsAlive)
                    continue;

                yield return hero;
            }
        }

        private static void OnHeroSelectionOver(List<InquiryElement> element)
        {
            if (element.First().Identifier is not Hero selectedHeir)
                return;

            SwitchPlayer(selectedHeir.Clan, selectedHeir);
        }

        internal static void SwitchPlayer(Clan selectedClan, Hero selectedHeir)
        {
            SyncBehavior.Instance.Clan = selectedClan;
            var oldLeader = selectedClan.Leader;
            var selectedNotLeader = selectedHeir != oldLeader;
            if (selectedNotLeader) ApplyWithSelectedNewLeader(selectedClan, selectedHeir);

            if (selectedHeir.CurrentSettlement is not null)
            {
                var mp = selectedHeir.PartyBelongedTo;
                if (mp is not null)
                {
                    mp.IsDisbanding = false;
                    LeaveSettlementAction.ApplyForCharacterOnly(selectedHeir);
                    LeaveSettlementAction.ApplyForParty(mp);
                }
            }

            ChangePlayerCharacterAction.Apply(selectedHeir);
            if (selectedHeir != oldLeader) ApplyWithSelectedNewLeader(selectedClan, oldLeader);
            MessageHelper.DisplayMessage($"Player Switched To {selectedHeir.Name}", Colors.Green);
        }

        private static void ApplyWithSelectedNewLeader(Clan clan, Hero? newLeader = null)
        {
            var leader = clan.Leader;
            if (newLeader is null)
            {
                var heirApparents = leader.Clan.GetHeirApparents();
                if (heirApparents.Count == 0)
                {
                    return;
                }
                var highestPoint = heirApparents.OrderByDescending(h => h.Value).FirstOrDefault().Value;
                newLeader = heirApparents.Where(h => h.Value.Equals(highestPoint)).GetRandomElementInefficiently().Key;
            }
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
            }
            leader.Clan.SetLeader(newLeader);
            if (GetCampaignEventDispatcher is not null)
                GetCampaignEventDispatcher(Campaign.Current).OnClanLeaderChanged(leader, newLeader);
        }
    }
}
