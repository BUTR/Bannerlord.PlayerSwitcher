using Bannerlord.PlayerSwitcher.Patches;
using Bannerlord.PlayerSwitcher.Utils;

using HarmonyLib.BUTR.Extensions;

using Helpers;

using MCM;

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
        private record SwitchEvent(Hero LastHero, MapEvent? MapEvent, SiegeEvent? SiegeEvent)
        {
            public static SwitchEvent CreateMapEvent(Hero lastHero, MapEvent? mapEvent) => new(lastHero, mapEvent, null);
            public static SwitchEvent CreateSiegeEvent(Hero lastHero, SiegeEvent siegeEvent) => new(lastHero, null, siegeEvent);
        }

        private delegate void OnCompanionAddedDelegate(Clan instance, Hero companion);
        private static readonly OnCompanionAddedDelegate? OnCompanionAdded =
            AccessTools2.GetDelegate<OnCompanionAddedDelegate>(typeof(Clan), "OnCompanionAdded");

        private delegate void OnCompanionRemovedDelegate(Clan instance, Hero companion);
        private static readonly OnCompanionRemovedDelegate? OnCompanionRemoved =
            AccessTools2.GetDelegate<OnCompanionRemovedDelegate>(typeof(Clan), "OnCompanionRemoved");

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
        private Settings? Settings => GetCampaignBehavior<SettingsProviderCampaignBehavior>() is { } behavior ? behavior.Get<Settings>() : null;

        private SwitchEvent? _switchEvent;

        public override void SyncData(IDataStore dataStore) { }

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
            // Still needed?
            if (MainParty.LeaderHero is null && !MainHero.IsPrisoner)
            {
                MainParty.MemberRoster.AddToCounts(MainHero.CharacterObject, 0);
            }
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
        {
            if (Settings is { SwitchMessages: false }) return;

            if (attackerParty.LeaderHero is { } attackerHero && attackerHero != MainHero && attackerHero.Clan == PlayerClan)
            {
                var text = Strings.AttackingSwitchTo
                    .SetTextVariable("LEADER", attackerHero.Name)
                    .SetTextVariable("DEFENDERS", defenderParty.Name);

                InformationManager.ShowInquiry(new InquiryData(
                        Strings.Attack.ToString(),
                        text.ToString(),
                        true,
                        true,
                        new TextObject("{=aeouhelq}Yes").ToString(),
                        new TextObject("{=8OkPHu4f}No").ToString(),
                        () =>
                        {
                            _switchEvent = SwitchEvent.CreateMapEvent(MainHero, mapEvent);
                            SwitchPlayer(attackerHero);
                        },
                        null),
                    true);
            }

            if (defenderParty.LeaderHero is { } defenderHero && defenderHero != MainHero && defenderHero.Clan == PlayerClan && defenderHero.PartyBelongedTo is { IsCaravan: false })
            {
                var text = Strings.AttackedBySwitchTo
                    .SetTextVariable("LEADER", defenderHero.Name)
                    .SetTextVariable("ATTACKERS", attackerParty.Name);

                InformationManager.ShowInquiry(new InquiryData(
                        Strings.Defend.ToString(),
                        text.ToString(),
                        true,
                        true,
                        new TextObject("{=aeouhelq}Yes").ToString(),
                        new TextObject("{=8OkPHu4f}No").ToString(),
                        () =>
                        {
                            _switchEvent = SwitchEvent.CreateMapEvent(MainHero, mapEvent);
                            SwitchPlayer(defenderHero);
                        },
                        null),
                    true);
            }
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (_switchEvent is not { } || _switchEvent.MapEvent != mapEvent) return;

            var lastHero = _switchEvent.LastHero;
            _switchEvent = null;

            var text = Strings.SwitchBack
                .SetTextVariable("LAST_HERO", lastHero.Name);

            InformationManager.ShowInquiry(new InquiryData(
                    Strings.BattleEnded.ToString(),
                    text.ToString(),
                    true,
                    true,
                    new TextObject("{=aeouhelq}Yes").ToString(),
                    new TextObject("{=8OkPHu4f}No").ToString(),
                    () => SwitchPlayer(lastHero),
                    null),
                true);
        }

        private void OnSiegeEventStarted(SiegeEvent siegeEvent)
        {
            if (!Settings?.SwitchMessages ?? false) return;

            var attackerParty = siegeEvent.BesiegerCamp.BesiegerParty;
            var besiegedSettlement = siegeEvent.BesiegedSettlement;

            if (attackerParty != MainParty && attackerParty.LeaderHero is { } leader && leader.Clan == PlayerClan)
            {
                var text = Strings.BesiegingSwitchTo
                    .SetTextVariable("LEADER", leader.Name)
                    .SetTextVariable("SETTLEMENT", besiegedSettlement.Name);

                InformationManager.ShowInquiry(new InquiryData(
                        Strings.Attack.ToString(),
                        text.ToString(),
                        true,
                        true,
                        new TextObject("{=aeouhelq}Yes").ToString(),
                        new TextObject("{=8OkPHu4f}No").ToString(),
                        () =>
                        {
                            _switchEvent = SwitchEvent.CreateSiegeEvent(MainHero, siegeEvent);
                            SwitchPlayer(leader);
                        },
                        null),
                    true);
            }

            if (besiegedSettlement.OwnerClan == PlayerClan && besiegedSettlement is { Town: { Governor: { } governor } })
            {
                var text = Strings.UnderSiegeSwitchTo
                    .SetTextVariable("LEADER", governor.Name)
                    .SetTextVariable("SETTLEMENT", besiegedSettlement.Name)
                    .SetTextVariable("ATTACKERS", attackerParty.Name);

                InformationManager.ShowInquiry(new InquiryData(
                        Strings.Defend.ToString(),
                        text.ToString(),
                        true,
                        true,
                        new TextObject("{=aeouhelq}Yes").ToString(),
                        new TextObject("{=8OkPHu4f}No").ToString(),
                        () =>
                        {
                            _switchEvent = SwitchEvent.CreateSiegeEvent(MainHero, siegeEvent);
                            SwitchPlayer(governor);
                        },
                        null),
                    true);
            }
        }

        private void OnSiegeEventEnded(SiegeEvent siegeEvent)
        {
            if (_switchEvent is not { } || _switchEvent.SiegeEvent != siegeEvent) return;

            var lastHero = _switchEvent.LastHero;
            _switchEvent = null;

            var text = Strings.SwitchBack
                .SetTextVariable("LAST_HERO", lastHero.Name);

            InformationManager.ShowInquiry(new InquiryData(
                    Strings.SiegeEnded.ToString(),
                    text.ToString(),
                    true,
                    true,
                    new TextObject("{=aeouhelq}Yes").ToString(),
                    new TextObject("{=8OkPHu4f}No").ToString(),
                    () => SwitchPlayer(lastHero),
                    null),
                true);
        }

        public void SwitchPlayer(Hero newLeader)
        {
            var oldLeader = MainHero;

            using (new AddRemoveCompanionActionHandler())
            {
                OnCompanionRemoved?.Invoke(newLeader.Clan, newLeader);
                OnCompanionAdded?.Invoke(oldLeader.Clan, oldLeader);
            }

            // We play the Hero now, he can't be a Governor anymore
            if (newLeader.GovernorOf is not null)
            {
                ChangeGovernorAction.Apply(newLeader.GovernorOf, null);
            }

            // Remove the hero from a caravan
            if (newLeader.PartyBelongedTo is { IsCaravan: true })
            {
                var settlement = SettlementHelper.FindNearestSettlement(s => (s.IsTown || s.IsCastle) && !FactionManager.IsAtWarAgainstFaction(s.MapFaction, newLeader.MapFaction)) ??
                                 SettlementHelper.FindNearestSettlement(s => s.IsVillage || (!s.IsHideout && !s.IsFortification));
                DestroyPartyAction.Apply(null, newLeader.PartyBelongedTo);
                TeleportHeroAction.ApplyImmediateTeleportToSettlement(newLeader, settlement);
            }

            // Move the hero outside the settlement
            if (newLeader.CurrentSettlement != null && newLeader.PartyBelongedTo != null)
            {
                LeaveSettlementAction.ApplyForCharacterOnly(newLeader);
                LeaveSettlementAction.ApplyForParty(newLeader.PartyBelongedTo);
            }

            // The current flow:
            // 1.   Game.Current.PlayerTroop is Hero.CharacterObject
            // 2.   Trigger CampaignEventDispatcher.Instance.OnBeforePlayerCharacterChanged
            // 2.1. HeirSelectionCampaignBehavior listens to it, we disable it via ChangePlayerCharacterActionHandler
            // 3.   Trigger Campaign.OnPlayerCharacterChanged
            // 3.1. Destroy n

            // 3.1. Campaign.MainParty is the Hero's party
            // 3.2. If the Party is not null, leave the settlement, else the Hero leaves
            // 3.3. Reassign Campaign.PlayerTraitDeveloper
            // 3.4. Create Campaign.MainParty if it didn't exists yet
            // 3.5. Other minor stuff
            // 4.   Trigger CampaignEventDispatcher.Instance.OnPlayerCharacterChanged
            // 4.1. HeirSelectionCampaignBehavior listens to it, we disable it via ChangePlayerCharacterActionHandler
            using (new ChangePlayerCharacterActionHandler())
            using (new DestroyPartyActionHandler())
            using (new LordPartyComponentHandler())
                ChangePlayerCharacterAction.Apply(newLeader);

            if (Settings is { CheatMode: true })
            {
                // Set Campaign.PlayerDefaultFaction
                SetPlayerDefaultFaction?.Invoke(Campaign, newLeader.Clan);

                var currentLeader = newLeader.Clan.Leader;
                if (newLeader != currentLeader)
                {
                    newLeader.Clan.SetLeader(newLeader);
                    GetCampaignEventDispatcher?.Invoke(Campaign).OnClanLeaderChanged(currentLeader, newLeader);
                }
            }

            MessageUtils.DisplayMessage(Strings.PlayerSwitchedTo.SetTextVariable("LEADER", newLeader.Name), Colors.Green);
        }
    }
}