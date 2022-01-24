using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "PlayerSwitcherSettings_v2";
        public override string FolderName => "PlayerSwitcher";
        public override string FormatType => "json2";
        public override string DisplayName => new TextObject("{=SW2ZF7BsYv}Player Switcher {VERSION}", new()
        {
            { "VERSION", typeof(Settings).Assembly.GetName().Version?.ToString(3) ?? "ERROR" }
        }).ToString();

        [SettingPropertyBool("{=qLPesYzLHy}Switch Messages", HintText = "{=f9cYRLIkLn}Enabling this will alert you when clan members are in battle so you can switch to them.", RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("{=qe3elfFGkN}Gameplay", GroupOrder = 0)]
        public bool SwitchMessages { get; set; } = true;
    }
}