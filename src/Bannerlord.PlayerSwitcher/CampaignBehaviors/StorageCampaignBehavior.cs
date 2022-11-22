using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher.CampaignBehaviors
{
    public class StorageCampaignBehavior : CampaignBehaviorBase
    {
        public static StorageCampaignBehavior? Instance { get; private set; }

        private readonly SwitchManager _switchManager = new();

        public Clan SelectedClan { get => _selectedClan; set => _selectedClan = value; }
        private Clan _selectedClan = default!;

        private Hero? lastHero;
        private MapEvent? clanEvent;
        private SiegeEvent? clanSiegeEvent;


        public StorageCampaignBehavior()
        {
            Instance = this;
        }

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
            if (MobileParty.MainParty.LeaderHero is null && !Hero.MainHero.IsPrisoner)
            {
                MobileParty.MainParty.MemberRoster.AddToCounts(Hero.MainHero.CharacterObject, 0);
            }
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase party1, PartyBase party2)
        {
            if (Settings.Instance is { SwitchMessages: false }) return;

            var hero = party1.LeaderHero;
            if (hero is { } && hero != Hero.MainHero && hero.Clan == Clan.PlayerClan)
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
                            lastHero = Hero.MainHero;
                            clanEvent = mapEvent;
                            _switchManager.SwitchPlayer(hero.Clan, hero);
                        },
                        null),
                    true);
                return;
            }

            hero = party2.LeaderHero;
            if (hero is { } && hero != Hero.MainHero && hero.Clan == Clan.PlayerClan)
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
                            lastHero = Hero.MainHero;
                            clanEvent = mapEvent;
                            _switchManager.SwitchPlayer(hero.Clan, hero);
                        },
                        null),
                    true);
                return;
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
                        _switchManager.SwitchPlayer(lastHero.Clan, lastHero);
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
            if (!Settings.Instance?.SwitchMessages ?? false) return;

            var attackerParty = siegeEvent.BesiegerCamp.BesiegerParty;
            var besiegedSettlement = siegeEvent.BesiegedSettlement;
            if (attackerParty != MobileParty.MainParty)
            {
                var leader = attackerParty.LeaderHero;
                if (leader.Clan == Clan.PlayerClan)
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
                                lastHero = Hero.MainHero;
                                clanSiegeEvent = siegeEvent;
                                _switchManager.SwitchPlayer(leader.Clan, leader);
                            },
                            null),
                        true);
                    return;
                }
            }

            if (besiegedSettlement.OwnerClan == Clan.PlayerClan)
            {
                var governor = besiegedSettlement.Town.Governor;
                if (governor is not null)
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
                                lastHero = Hero.MainHero;
                                clanSiegeEvent = siegeEvent;
                                _switchManager.SwitchPlayer(governor.Clan, governor);
                            },
                            null),
                        true);
                    return;
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
                        _switchManager.SwitchPlayer(lastHero.Clan, lastHero);
                        lastHero = null;
                    },
                    () =>
                    {
                        lastHero = null;
                    }),
                true);
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("Clan", ref _selectedClan);
            }
            catch (Exception)
            {
            }
        }
    }
}