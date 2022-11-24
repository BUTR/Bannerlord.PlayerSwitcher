using Bannerlord.ButterLib.HotKeys;

using TaleWorlds.InputSystem;

using HotKeyManager = Bannerlord.ButterLib.HotKeys.HotKeyManager;

namespace Bannerlord.PlayerSwitcher.HotKeys
{
    public class SwitchKey : HotKeyBase
    {
        protected override string DisplayName => Strings.HotKeySwitch;
        protected override string Description => Strings.HotKeySwitchDescription;
        protected override InputKey DefaultKey => InputKey.End;
        protected override string Category { get; } = HotKeyManager.Categories[HotKeyCategory.CampaignMap];

        public SwitchKey() : base(nameof(SwitchKey)) { }
    }
}