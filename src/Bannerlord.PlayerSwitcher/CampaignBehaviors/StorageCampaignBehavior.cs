using Bannerlord.PlayerSwitcher.Patches;
using Bannerlord.PlayerSwitcher.Utils;

using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher.CampaignBehaviors
{
    public class StorageCampaignBehavior : CampaignBehaviorBase
    {
        private delegate CampaignEventDispatcher GetCampaignEventDispatcherDelegate(Campaign instance);
        private static readonly GetCampaignEventDispatcherDelegate? GetCampaignEventDispatcher =
            AccessTools2.GetPropertyGetterDelegate<GetCampaignEventDispatcherDelegate>(typeof(Campaign), "CampaignEventDispatcher");

        private delegate void SetPlayerDefaultFactionDelegate(Campaign instance, Clan value);
        private static readonly SetPlayerDefaultFactionDelegate? SetPlayerDefaultFaction =
            AccessTools2.GetPropertySetterDelegate<SetPlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        private Hero MainHero => Hero.MainHero;
        private MobileParty MainParty => MobileParty.MainParty;
        private Clan PlayerClan => Clan.PlayerClan;
        private Campaign Campaign => Campaign.Current;
        private Settings? Settings => Settings.Instance;
        
        public Clan SelectedClan { get => _selectedClan; set => _selectedClan = value; }
        private Clan _selectedClan = default!;

        private Hero? lastHero;
        private MapEvent? clanEvent;
        private SiegeEvent? clanSiegeEvent;


        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, OnSiegeEventStarted);
            CampaignEvents.OnSiegeEventEndedEvent.AddNonSerializedListener(this, OnSiegeEventEnded);
        }

        private void OnTick(float dt)
        {
            if (MainParty.LeaderHero is null && !MainHero.IsPrisoner)
            {
                MainParty.MemberRoster.AddToCounts(MainHero.CharacterObject, 0);
            }
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase party1, PartyBase party2)
        {
            if (Settings is { SwitchMessages: false }) return;

            var hero = party1.LeaderHero;
            if (hero is { } && hero != MainHero && hero.Clan == PlayerClan)
            {
                var text = new TextObject("{=QKCOKf5jRM}{LEADER} is attacking {DEFENDERS}. Switch to {LEADER}?")
                    .SetTextVariable("LEADER", hero.Name)
                    .SetTextVariable("DEFENDERS", party2.Name);

                InformationManager.ShowInquiry(new InquiryData(
                        new TextObject("{=qKus0BE3xi}Attack").ToString(),
                        text.ToString(),
                        true,
                        true,
                        new TextObject("{=aeouhelq}Yes").ToString(),
                        new TextObject("{=8OkPHu4f}No").ToString(),
                        () =>
                        {
                            lastHero = MainHero;
                            clanEvent = mapEvent;
                            SwitchPlayer(hero.Clan, hero);
                        },
                        null),
                    true);
                return;
            }

            hero = party2.LeaderHero;
            if (hero is { } && hero != MainHero && hero.Clan == PlayerClan)
            {
                var text = new TextObject("{=jOyHIwxmhb}{LEADER} is being attacked by {ATTACKERS}. Switch to {LEADER}?")
                    .SetTextVariable("LEADER", hero.Name)
                    .SetTextVariable("ATTACKERS", party1.Name);

                InformationManager.ShowInquiry(new InquiryData(
                        new TextObject("{=pai08I9LP2}Defend").ToString(),
                        text.ToString(),
                        true,
                        true,
                        new TextObject("{=aeouhelq}Yes").ToString(),
                        new TextObject("{=8OkPHu4f}No").ToString(),
                        () =>
                        {
                            lastHero = MainHero;
                            clanEvent = mapEvent;
                            SwitchPlayer(hero.Clan, hero);
                        },
                        null),
                    true);
                //return;
            }
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent != clanEvent || lastHero is null) return;

            clanEvent = null;
            var text = new TextObject("{=DMtEOm8C2W}Switch back to {LAST_HERO}?")
                .SetTextVariable("LAST_HERO", lastHero.Name);

            InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=1Gfj29qE2R}Battle Ended").ToString(),
                    text.ToString(),
                    true,
                    true,
                    new TextObject("{=aeouhelq}Yes").ToString(),
                    new TextObject("{=8OkPHu4f}No").ToString(),
                    () =>
                    {
                        SwitchPlayer(lastHero.Clan, lastHero);
                        lastHero = null;
                    },
                    () =>
                    {
                        lastHero = null;
                    }),
                true);
        }

        private void OnSiegeEventStarted(SiegeEvent siegeEvent)
        {
            if (!Settings?.SwitchMessages ?? false) return;

            var attackerParty = siegeEvent.BesiegerCamp.BesiegerParty;
            var besiegedSettlement = siegeEvent.BesiegedSettlement;
            if (attackerParty != MainParty)
            {
                var leader = attackerParty.LeaderHero;
                if (leader.Clan == PlayerClan)
                {
                    var text = new TextObject("{=G0pTcJD6cG}{LEADER} is besieging {SETTLEMENT}. Switch to {LEADER}?")
                        .SetTextVariable("LEADER", leader.Name)
                        .SetTextVariable("SETTLEMENT", besiegedSettlement.Name);

                    InformationManager.ShowInquiry(new InquiryData(
                            new TextObject("{=qKus0BE3xi}Attack").ToString(),
                            text.ToString(),
                            true,
                            true,
                            new TextObject("{=aeouhelq}Yes").ToString(),
                            new TextObject("{=8OkPHu4f}No").ToString(),
                            () =>
                            {
                                lastHero = MainHero;
                                clanSiegeEvent = siegeEvent;
                                SwitchPlayer(leader.Clan, leader);
                            },
                            null),
                        true);
                    return;
                }
            }

            if (besiegedSettlement.OwnerClan == PlayerClan)
            {
                if (besiegedSettlement.Town.Governor is { } governor)
                {
                    var text = new TextObject("{=OHKrGhoLp3}{SETTLEMENT} is under siege by {ATTACKERS}. Switch to {LEADER}?");
                    text.SetTextVariable("LEADER", governor.Name);
                    text.SetTextVariable("SETTLEMENT", besiegedSettlement.Name);
                    text.SetTextVariable("ATTACKERS", attackerParty.Name);

                    InformationManager.ShowInquiry(new InquiryData(
                            new TextObject("{=pai08I9LP2}Defend").ToString(),
                            text.ToString(),
                            true,
                            true,
                            new TextObject("{=aeouhelq}Yes").ToString(),
                            new TextObject("{=8OkPHu4f}No").ToString(),
                            () =>
                            {
                                lastHero = MainHero;
                                clanSiegeEvent = siegeEvent;
                                SwitchPlayer(governor.Clan, governor);
                            },
                            null),
                        true);
                    //return;
                }
            }
        }

        private void OnSiegeEventEnded(SiegeEvent siegeEvent)
        {
            if (siegeEvent != clanSiegeEvent || lastHero is null) return;

            clanSiegeEvent = null;
            var text = new TextObject("{=FxvQbHocXL}Switch back to {LAST_HERO}?")
                .SetTextVariable("LAST_HERO", lastHero.Name);

            InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=lbx4kAzf0A}Siege Ended").ToString(),
                    text.ToString(),
                    true,
                    true,
                    new TextObject("{=aeouhelq}Yes").ToString(),
                    new TextObject("{=8OkPHu4f}No").ToString(),
                    () =>
                    {
                        SwitchPlayer(lastHero.Clan, lastHero);
                        lastHero = null;
                    },
                    () =>
                    {
                        lastHero = null;
                    }),
                true);
        }

        public void SwitchPlayer(Clan selectedClan, Hero newLeader)
        {
            SelectedClan = selectedClan;

            // We play the Hero now, he can't be a Governor anymore
            if (newLeader.GovernorOf is not null)
            {
                ChangeGovernorAction.Apply(newLeader.GovernorOf, null);
            }

            // The current flow:
            // 1.   Game.Current.PlayerTroop is Hero.CharacterObject
            // 2.   Trigger CampaignEventDispatcher.Instance.OnBeforePlayerCharacterChanged
            // 2.1. HeirSelectionCampaignBehavior listens to it, we disable it via ChangePlayerCharacterActionHandler
            // 3.   Trigger Campaign.OnPlayerCharacterChanged
            // 3.1. Campaign.MainParty is the Hero's party
            // 3.2. If the Party is not null, leave the settlement, else the Hero leaves
            // 3.3. Reassign Campaign.PlayerTraitDeveloper
            // 3.4. Create Campaign.MainParty if it didn't exists yet
            // 3.5. Other minor stuff
            // 4.   Trigger CampaignEventDispatcher.Instance.OnPlayerCharacterChanged
            // 4.1. HeirSelectionCampaignBehavior listens to it, we disable it via ChangePlayerCharacterActionHandler
            using (new ChangePlayerCharacterActionHandler())
                ChangePlayerCharacterAction.Apply(newLeader);

            // Set Campaign.PlayerDefaultFaction
            if (SetPlayerDefaultFaction is not null)
                SetPlayerDefaultFaction(Campaign, selectedClan);

            if (Settings is { CheatMode: true })
            {
                var currentLeader = selectedClan.Leader;
                if (newLeader != currentLeader)
                {
                    selectedClan.SetLeader(newLeader);
                    if (GetCampaignEventDispatcher is not null)
                        GetCampaignEventDispatcher(Campaign).OnClanLeaderChanged(currentLeader, newLeader);
                }
            }

            MessageUtils.DisplayMessage(new TextObject("{=nqwp2XsNFW}Player Switched To {LEADER}").SetTextVariable("LEADER", newLeader.Name), Colors.Green);
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("Clan", ref _selectedClan);
            }
            catch (Exception) { /* ignored */ }
        }
    }
}