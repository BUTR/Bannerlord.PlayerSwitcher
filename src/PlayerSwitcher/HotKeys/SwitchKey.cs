using Bannerlord.ButterLib.HotKeys;

using TaleWorlds.CampaignSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

using HotKeyManager = Bannerlord.ButterLib.HotKeys.HotKeyManager;


namespace PlayerSwitcher.HotKeys
{
    internal class SwitchKey : HotKeyBase
    {
        protected override string DisplayName { get; }
        protected override string Description { get; }
        protected override InputKey DefaultKey { get; }
        protected override string Category { get; }


        public SwitchKey() : base(nameof(SwitchKey))
        {
            DisplayName = "Switch Player Hero";
            Description = "We understand how dangerous a mask can be. We all become what we pretend to be.";
            DefaultKey = InputKey.End;
            Category = HotKeyManager.Categories[HotKeyCategory.CampaignMap];
            Predicate = IsKeyActive;
        }

        private bool IsKeyActive()
        {
            return Campaign.Current is not null && Mission.Current is null;
        }

        protected override void OnPressed()
        {
            if (Hero.MainHero.CurrentSettlement is not null)
            {
                MessageHelper.DisplayMessage("Leave your current settlement before switching players so the game can close the menu and unload the interface at the top of the screen that shows all the notables.");
                return;
            }

            SubModule.SelectClan();
        }
    }
}