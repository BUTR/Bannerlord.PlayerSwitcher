using Bannerlord.ButterLib.Extensions;
using Bannerlord.PlayerSwitcher.CampaignBehaviors;
using Bannerlord.PlayerSwitcher.HotKeys;
using Bannerlord.PlayerSwitcher.Utils;

using Helpers;

using SandBox.View.Map;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.PlayerSwitcher.ScreenLayers
{
    /// <summary>
    /// A HotKey and logic handler that will bind itself to the <see cref="MapScreen"/>.
    /// We basically use a <see cref="ScreenLayer"/> just for the <see cref="ScreenLayer.Input"/> so we can subscribe to the HotKey event.
    /// Plus binding to MapScreen will help us ignore input handling when it shouldn't occur.
    /// </summary>
    public class SwitchManagerMapScreenLayer : ScreenLayer
    {
        private readonly StorageCampaignBehavior _storageCampaignBehavior;
        private readonly IDisposable? _subscription;

        public SwitchManagerMapScreenLayer(StorageCampaignBehavior storageCampaignBehavior) : base(0, string.Empty)
        {
            _storageCampaignBehavior = storageCampaignBehavior;
            _subscription = Input.SubscribeToIsDownAndReleasedEvent<SwitchKey>(() =>
            {
                if (!IsActive) return;
                if (Settings.Instance is not { } settings) return;
                if (Campaign.Current is null) return;
                if (Mission.Current is not null) return;

                if (Hero.MainHero.CurrentSettlement is not null)
                {
                    MessageUtils.DisplayMessage(new TextObject("{=WQQBeAQcyc}Leave your current settlement before switching players so the game can close the menu and unload the interface at the top of the screen that shows all the notables."));
                    return;
                }

                if (settings.CheatMode)
                {
                    SelectClan();
                }
                else
                {
                    SelectPlayerClanHeroes();
                }
            });
        }

        private void SelectClan()
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

            MBInformationManager.ShowMultiSelectionInquiry(
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

        private void SelectPlayerClanHeroes()
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

            var inquiries = ClanInquiries(Clan.PlayerClan).ToList();
            if (inquiries.Count == 0)
            {
                MessageUtils.DisplayMessage(new TextObject("{=aqTKT8UyBg}You don't have anyone to switch to!"), Colors.Green);
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(
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
                MessageUtils.DisplayMessage(new TextObject("{=aqTKT8UyBg}You don't have anyone to switch to!"), Colors.Green);
                return;
            }

            MBInformationManager.ShowMultiSelectionInquiry(
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

            _storageCampaignBehavior.SwitchPlayer(selectedHeir.Clan, selectedHeir);
        }

        private static IEnumerable<Hero> GetHeroes(Clan clan)
        {
            foreach (var hero in clan.Heroes)
            {
                if (hero is null)
                    continue;

                if (!hero.IsAlive)
                    continue;

                if (hero == Hero.MainHero)
                    continue;

                if (hero.PartyBelongedTo is null || hero.PartyBelongedTo.LeaderHero != hero)
                    continue;

                yield return hero;
            }
        }

        protected override void OnFinalize()
        {
            _subscription?.Dispose();

            base.OnFinalize();
        }
    }
}