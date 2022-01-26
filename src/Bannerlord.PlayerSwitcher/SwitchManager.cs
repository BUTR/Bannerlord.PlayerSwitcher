using Bannerlord.PlayerSwitcher.CampaignBehaviors;
using Bannerlord.PlayerSwitcher.Patches;

using HarmonyLib.BUTR.Extensions;

using Helpers;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher
{
    public sealed class SwitchManager
    {
        private delegate CampaignEventDispatcher GetCampaignEventDispatcherDelegate(Campaign instance);
        private static readonly GetCampaignEventDispatcherDelegate? GetCampaignEventDispatcher =
            AccessTools2.GetPropertyGetterDelegate<GetCampaignEventDispatcherDelegate>(typeof(Campaign), "CampaignEventDispatcher");

        private delegate void SetPlayerDefaultFactionDelegate(Campaign instance, Clan value);
        private static readonly SetPlayerDefaultFactionDelegate? SetPlayerDefaultFaction =
            AccessTools2.GetPropertySetterDelegate<SetPlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        public static SwitchManager? Instance { get; private set; }

        public SwitchManager()
        {
            Instance = this;
        }

        public void SelectClan()
        {
            static IEnumerable<Clan> GetAllHeirApparents()
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

            static IEnumerable<InquiryElement> ClanInquiries()
            {
                foreach (var clan in GetAllHeirApparents().OrderBy(x => x.Tier))
                {
                    if (clan.StringId == "neutral")
                        continue;

                    yield return new InquiryElement(clan, clan.Name.ToString(), new ImageIdentifier(clan.Banner));
                }
            }

            InformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("{=VfJiuott1b}Player Switcher").ToString(),
                    new TextObject("{=3H7NmYfKn6}Select a clan to choose from their heroes.").ToString(),
                    ClanInquiries().ToList(),
                    true,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnFactionSelectionOver,
                    null)
            );

            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
        }

        public void SelectPlayerClanHeroes()
        {
            static IEnumerable<Hero> GetHeroes(Clan clan)
            {
                foreach (var hero in clan.Heroes)
                {
                    if (hero is null)
                        continue;

                    if (!hero.IsAlive)
                        continue;

                    if (hero == Hero.MainHero)
                        continue;

                    if (hero.CompanionOf is not null)
                        continue;

                    if (hero.PartyBelongedTo is not null && hero.PartyBelongedTo.LeaderHero != hero)
                        continue;

                    yield return hero;
                }
            }

            static IEnumerable<InquiryElement> ClanInquiries(Clan clan)
            {
                foreach (var hero in GetHeroes(clan))
                {
                    var parent = new TextObject("{HERO.NAME}");
                    StringHelpers.SetCharacterProperties("HERO", hero.CharacterObject, parent);
                    yield return new InquiryElement(hero, parent.ToString(), new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject)));
                }
            }

            var inquiries = ClanInquiries(Clan.PlayerClan).ToList();
            if (inquiries.Count == 0)
            {
                MessageHelper.DisplayMessage(new TextObject("{=aqTKT8UyBg}You don't have anyone to switch to!"), Colors.Green);
                return;
            }

            InformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("{=VfJiuott1b}Player Switcher").ToString(),
                    new TextObject("{=yP5F99s3ti}Select a hero to play as.").ToString(),
                    inquiries,
                    true,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );

            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
        }

        private void OnFactionSelectionOver(List<InquiryElement> element)
        {
            static IEnumerable<Hero> GetHeroes(Clan clan)
            {
                foreach (var hero in clan.Heroes)
                {
                    if (hero is null)
                        continue;

                    if (!hero.IsAlive)
                        continue;

                    if (hero == Hero.MainHero)
                        continue;

                    if (hero.CompanionOf is not null)
                        continue;

                    if (hero.PartyBelongedTo is null || hero.PartyBelongedTo.LeaderHero != hero)
                        continue;

                    yield return hero;
                }
            }

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

            var inquiries = ClanInquiries(clan).ToList();
            if (inquiries.Count == 0)
            {
                MessageHelper.DisplayMessage(new TextObject("{=aqTKT8UyBg}You don't have anyone to switch to!"), Colors.Green);
                return;
            }

            InformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    new TextObject("{=VfJiuott1b}Player Switcher").ToString(),
                    new TextObject("{=yP5F99s3ti}Select a hero to play as.").ToString(),
                    inquiries,
                    true,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );
        }

        private void OnHeroSelectionOver(List<InquiryElement> element)
        {
            if (element.First().Identifier is not Hero selectedHeir) return;

            SwitchPlayer(selectedHeir.Clan, selectedHeir);
        }

        public void SwitchPlayer(Clan selectedClan, Hero newLeader)
        {
            if (StorageCampaignBehavior.Instance is not { } storageCampaignBehavior) return;

            storageCampaignBehavior.SelectedClan = selectedClan;

            // We play the Hero now, he can't be a Governor anymore
            if (newLeader.GovernorOf is not null)
            {
                ChangeGovernorAction.Apply(newLeader.GovernorOf, null);
            }

            // The current flow:
            // 1.   Game.Current.PlayerTroop is Hero.CharacterObject
            // 2.   Trigger CampaignEventDispatcher.Instance.OnBeforePlayerCharacterChanged
            // 2.1. HeirSelectionCampaignBehavior listens to it, we disable it via ChangePlayerCharacterActionHandler
            // 3.   Trigger Campaign.Current.OnPlayerCharacterChanged
            // 3.1. Campaign.Current.MainParty is the Hero's party
            // 3.2. If the Party is not null, leave the settlement, else the Hero leaves
            // 3.3. Reassign Campaign.Current.PlayerTraitDeveloper
            // 3.4. Create Campaign.Current.MainParty if it didn't exists yet
            // 3.5. Other minor stuff
            // 4.   Trigger CampaignEventDispatcher.Instance.OnPlayerCharacterChanged
            // 4.1. HeirSelectionCampaignBehavior listens to it, we disable it via ChangePlayerCharacterActionHandler
            using (new ChangePlayerCharacterActionHandler())
                ChangePlayerCharacterAction.Apply(newLeader);

            // Set Campaign.Current.PlayerDefaultFaction
            if (SetPlayerDefaultFaction is not null)
                SetPlayerDefaultFaction(Campaign.Current, selectedClan);

            if (Settings.Instance is { CheatMode: true })
            {
                var currentLeader = selectedClan.Leader;
                if (newLeader != currentLeader)
                {
                    selectedClan.SetLeader(newLeader);
                    if (GetCampaignEventDispatcher is not null)
                        GetCampaignEventDispatcher(Campaign.Current).OnClanLeaderChanged(currentLeader, newLeader);
                }
            }

            MessageHelper.DisplayMessage(new TextObject("{=nqwp2XsNFW}Player Switched To {LEADER}").SetTextVariable("LEADER", newLeader.Name), Colors.Green);
        }
    }
}