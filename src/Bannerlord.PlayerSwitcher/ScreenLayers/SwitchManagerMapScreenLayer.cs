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
using TaleWorlds.CampaignSystem.GameState;
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

                if (Game.Current.GameStateManager.LastOrDefault<MapState>() is { AtMenu: true })
                {
                    MessageUtils.DisplayMessage(Strings.ExitMenu);
                    return;
                }

                if (Hero.MainHero.CurrentSettlement is not null)
                {
                    MessageUtils.DisplayMessage(Strings.LeaveSettlement);
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

        protected override void OnFinalize()
        {
            _subscription?.Dispose();

            base.OnFinalize();
        }

        private void SelectClan()
        {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    Strings.PlayerSwitcher.ToString(),
                    Strings.SelectAClanToPlay.ToString(),
                    ClanHeirInquiries().ToList(),
                    true,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnFactionSelectionOver,
                    null)
            );
#elif v120
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    Strings.PlayerSwitcher.ToString(),
                    Strings.SelectAClanToPlay.ToString(),
                    ClanHeirInquiries().ToList(),
                    true,
                    1,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnFactionSelectionOver,
                    null)
            );
#else
#error Error
#endif

            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
        }

        private void SelectPlayerClanHeroes()
        {
            var inquiries = ClanInquiries(Clan.PlayerClan).ToList();
            if (inquiries.Count == 0)
            {
                MessageUtils.DisplayMessage(Strings.NoneToSwitch, Colors.Green);
                return;
            }

#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    Strings.PlayerSwitcher.ToString(),
                    Strings.SelectAHeroToPlay.ToString(),
                    inquiries,
                    true,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );
#elif v120
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    Strings.PlayerSwitcher.ToString(),
                    Strings.SelectAHeroToPlay.ToString(),
                    inquiries,
                    true,
                    1,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );
#else
#error Error
#endif

            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
        }

        private void OnFactionSelectionOver(List<InquiryElement> element)
        {
            if (element.First().Identifier is not Clan clan)
                return;

            var inquiries = ClanInquiries(clan).ToList();
            if (inquiries.Count == 0)
            {
                MessageUtils.DisplayMessage(Strings.NoneToSwitch, Colors.Green);
                return;
            }

#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    Strings.PlayerSwitcher.ToString(),
                    Strings.SelectAHeroToPlay.ToString(),
                    inquiries,
                    true,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );
#elif v120
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    Strings.PlayerSwitcher.ToString(),
                    Strings.SelectAHeroToPlay.ToString(),
                    inquiries,
                    true,
                    1,
                    1,
                    new TextObject("{=WiNRdfsm}Done").ToString(),
                    "",
                    OnHeroSelectionOver,
                    null)
            );
#else
#error Error
#endif
        }

        private void OnHeroSelectionOver(List<InquiryElement> element)
        {
            if (element.First().Identifier is not Hero selectedHeir) return;

            _storageCampaignBehavior.SwitchPlayer(selectedHeir);
        }

        private static IEnumerable<InquiryElement> ClanInquiries(Clan clan)
        {
            foreach (var hero in GetHeroes(clan))
            {
                var parent = new TextObject("{HERO.NAME}");
                StringHelpers.SetCharacterProperties("HERO", hero.CharacterObject, parent);
                yield return new InquiryElement(hero, parent.ToString(), new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject)));
            }
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


        private static IEnumerable<InquiryElement> ClanHeirInquiries()
        {
            foreach (var clan in GetAllHeirApparents().OrderBy(x => x.Tier))
            {
                if (clan.StringId == "neutral")
                    continue;

                yield return new InquiryElement(clan, clan.Name.ToString(), new ImageIdentifier(clan.Banner));
            }
        }
        private static IEnumerable<Clan> GetAllHeirApparents()
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
    }
}