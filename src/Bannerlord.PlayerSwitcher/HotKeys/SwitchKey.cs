using Bannerlord.ButterLib.HotKeys;

using TaleWorlds.InputSystem;

using HotKeyManager = Bannerlord.ButterLib.HotKeys.HotKeyManager;

namespace Bannerlord.PlayerSwitcher.HotKeys
{
    internal class SwitchKey : HotKeyBase
    {
        protected override string DisplayName => "{=Ooz4Z5vMZR}Switch Player Hero";
        protected override string Description => "{CGoVgCVK7t=}Switches the currently played hero.";
        protected override InputKey DefaultKey => InputKey.End;
        protected override string Category { get; } = HotKeyManager.Categories[HotKeyCategory.CampaignMap];

        public SwitchKey() : base(nameof(SwitchKey)) { }
    }
}