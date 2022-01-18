using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace PlayerSwitcher
{
    internal class SyncBehavior : CampaignBehaviorBase
    {
        public static SyncBehavior Instance { get; private set; }

        [SaveableField(0)]
        public Clan Clan;

        private Hero lastHero;
        private MapEvent clanEvent;
        private SiegeEvent clanSiegeEvent;


        public SyncBehavior()
        {
            Instance = this;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, _ =>
            {
                if (MobileParty.MainParty.LeaderHero is null && !Hero.MainHero.IsPrisoner)
                {
                    MobileParty.MainParty.MemberRoster.AddToCounts(Hero.MainHero.CharacterObject, 0);
                }
            });

            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, (mapEvent, party1, party2) =>
            {
                if (!Settings.Instance?.SwitchMessages ?? false) return;

                var hero = party1.LeaderHero;
                if (hero is not null && hero != Hero.MainHero && hero.Clan == Clan.PlayerClan)
                {
                    var text = new TextObject("{LEADER} is attacking {DEFENDERS}. Switch to {LEADER}?");
                    text.SetTextVariable("LEADER", hero.Name);
                    text.SetTextVariable("DEFENDERS", party2.Name);

                    InformationManager.ShowInquiry(new InquiryData("Attack", text.ToString(), true, true, "Yes", "No",
                        () =>
                        {
                            lastHero = Hero.MainHero;
                            clanEvent = mapEvent;
                            SubModule.SwitchPlayer(hero.Clan, hero);
                        }, null), true);
                    return;
                }

                hero = party2.LeaderHero;
                if (hero is not null && hero != Hero.MainHero && hero.Clan == Clan.PlayerClan)
                {
                    var text = new TextObject("{LEADER} is being attacked by {ATTACKERS}. Switch to {LEADER}?");
                    text.SetTextVariable("LEADER", hero.Name);
                    text.SetTextVariable("ATTACKERS", party1.Name);

                    InformationManager.ShowInquiry(new InquiryData("Defend", text.ToString(), true, true, "Yes", "No",
                        () =>
                        {
                            lastHero = Hero.MainHero;
                            clanEvent = mapEvent;
                            SubModule.SwitchPlayer(hero.Clan, hero);
                        }, null), true);
                    return;
                }
            });

            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, mapEvent =>
            {
                if (mapEvent != clanEvent || lastHero is null) return;

                clanEvent = null;
                var text = new TextObject("Switch back to {LAST_HERO}?");
                text.SetTextVariable("LAST_HERO", lastHero.Name);

                InformationManager.ShowInquiry(new InquiryData("Battle Ended", text.ToString(), true, true, "Yes", "No",
                    () =>
                    {
                        SubModule.SwitchPlayer(lastHero.Clan, lastHero);
                        lastHero = null;
                    },
                    () =>
                    {
                        lastHero = null;
                    }), true);
            });

            CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, siegeEvent =>
            {
                if (!Settings.Instance?.SwitchMessages ?? false) return;

                var attackerParty = siegeEvent.BesiegerCamp.BesiegerParty;
                var besiegedSettlement = siegeEvent.BesiegedSettlement;
                if (attackerParty != MobileParty.MainParty)
                {
                    var leader = attackerParty.LeaderHero;
                    if (leader.Clan == Clan.PlayerClan)
                    {
                        var text = new TextObject("{LEADER} is besieging {SETTLEMENT}. Switch to {LEADER}?");
                        text.SetTextVariable("LEADER", leader.Name);
                        text.SetTextVariable("SETTLEMENT", besiegedSettlement.Name);

                        InformationManager.ShowInquiry(new InquiryData("Attack", text.ToString(), true, true, "Yes", "No",
                            () =>
                            {
                                lastHero = Hero.MainHero;
                                clanSiegeEvent = siegeEvent;
                                SubModule.SwitchPlayer(leader.Clan, leader);
                            }, null), true);
                        return;
                    }
                }

                if (besiegedSettlement.OwnerClan == Clan.PlayerClan)
                {
                    var governor = besiegedSettlement.Town.Governor;
                    if (governor is not null)
                    {
                        var text = new TextObject("{SETTLEMENT} is under siege by {ATTACKERS}. Switch to {LEADER}?");
                        text.SetTextVariable("LEADER", governor.Name);
                        text.SetTextVariable("SETTLEMENT", besiegedSettlement.Name);
                        text.SetTextVariable("ATTACKERS", attackerParty.Name);

                        InformationManager.ShowInquiry(new InquiryData("Defend", text.ToString(), true, true, "Yes", "No",
                            () =>
                            {
                                lastHero = Hero.MainHero;
                                clanSiegeEvent = siegeEvent;
                                SubModule.SwitchPlayer(governor.Clan, governor);
                            }, null), true);
                        return;
                    }
                }
            });

            CampaignEvents.OnSiegeEventEndedEvent.AddNonSerializedListener(this, siegeEvent =>
            {
                if (siegeEvent != clanSiegeEvent || lastHero is null) return;

                clanSiegeEvent = null;
                var text = new TextObject("Switch back to {LAST_HERO}?");
                text.SetTextVariable("LAST_HERO", lastHero.Name);

                InformationManager.ShowInquiry(new InquiryData("Siege Ended", text.ToString(), true, true, "Yes", "No",
                    () =>
                    {
                        SubModule.SwitchPlayer(lastHero.Clan, lastHero);
                        lastHero = null;
                    },
                    () =>
                    {
                        lastHero = null;
                    }), true);
            });
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("Clan", ref Clan);
            }
            catch (Exception)
            {
            }
        }
    }
}