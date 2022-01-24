using Bannerlord.ButterLib.HotKeys;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

using HotKeyManager = Bannerlord.ButterLib.HotKeys.HotKeyManager;

namespace Bannerlord.PlayerSwitcher.HotKeys
{
    internal class SwitchKey : HotKeyBase
    {
        protected override string DisplayName { get; }
        protected override string Description { get; }
        protected override InputKey DefaultKey { get; }
        protected override string Category { get; }


        public SwitchKey() : base(nameof(SwitchKey))
        {
            DisplayName = "{=Ooz4Z5vMZR}Switch Player Hero";
            Description = "{CGoVgCVK7t=}Switches the currently played hero.";
            DefaultKey = InputKey.End;
            Category = HotKeyManager.Categories[HotKeyCategory.CampaignMap];
        }

        protected override void OnPressed()
        {
            if (SwitchManager.Instance is null) return;
            if (Campaign.Current is null) return;
            if (Mission.Current is not null) return;

            if (Hero.MainHero.CurrentSettlement is not null)
            {
                MessageHelper.DisplayMessage(new TextObject("{=WQQBeAQcyc}Leave your current settlement before switching players so the game can close the menu and unload the interface at the top of the screen that shows all the notables."));
                return;
            }

            SwitchManager.Instance.SelectClan();
        }
    }
}