using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v1;
using MCM.Abstractions.Settings.Base.Global;

namespace PlayerSwitcher
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id { get; } = "PlayerSwitcherSettings_v2";
        public override string DisplayName { get; } = "Player Switcher";
        public override string FolderName { get; } = "PlayerSwitcher";
        public override string FormatType { get; } = "json2";

        [SettingProperty("Switch Messages", HintText = "Enabling this will alert you when clan members are in battle so you can switch to them.", RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("Gameplay", GroupOrder = 0)]
        public bool SwitchMessages { get; set; } = true;
    }
}