using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher
{
    public class Strings
    {
        public static readonly string HotKeySwitch = "{=Ooz4Z5vMZR}Switch Player Hero";
        public static readonly string HotKeySwitchDescription = "{CGoVgCVK7t=}Switches the currently played hero.";

        public static readonly TextObject PlayerSwitcher = new("{=VfJiuott1b}Player Switcher");

        public static readonly TextObject PosseOfText = new("{=CjU71TGHWq}Posse of {LEADER}");
        public static readonly TextObject AttackingSwitchTo = new("{=QKCOKf5jRM}{LEADER} is attacking {DEFENDERS}. Switch to {LEADER}?");
        public static readonly TextObject AttackedBySwitchTo = new("{=jOyHIwxmhb}{LEADER} is being attacked by {ATTACKERS}. Switch to {LEADER}?");
        public static readonly TextObject BesiegingSwitchTo = new("{=G0pTcJD6cG}{LEADER} is besieging {SETTLEMENT}. Switch to {LEADER}?");
        public static readonly TextObject UnderSiegeSwitchTo = new("{=OHKrGhoLp3}{SETTLEMENT} is under siege by {ATTACKERS}. Switch to {LEADER}?");
        public static readonly TextObject SwitchBack = new("{=DMtEOm8C2W}Switch back to {LAST_HERO}?");
        public static readonly TextObject PlayerSwitchedTo = new("{=nqwp2XsNFW}Player Switched To {LEADER}");

        public static readonly TextObject Attack = new("{=qKus0BE3xi}Attack");
        public static readonly TextObject Defend = new("{=pai08I9LP2}Defend");

        public static readonly TextObject BattleEnded = new("{=1Gfj29qE2R}Battle Ended");
        public static readonly TextObject SiegeEnded = new("{=lbx4kAzf0A}Siege Ended");

        public static readonly TextObject LeaveSettlement = new("{=WQQBeAQcyc}Leave your current settlement before switching players so the game can close the menu and unload the interface at the top of the screen that shows all the notables.");
        public static readonly TextObject ExitMenu = new("{=JHnsYgSgtS}Close the menu open before switching.");

        public static readonly TextObject NoneToSwitch = new("{=aqTKT8UyBg}You don't have anyone to switch to!");

        public static readonly TextObject SelectAHeroToPlay = new("{=yP5F99s3ti}Select a hero to play as.");
        public static readonly TextObject SelectAClanToPlay = new TextObject("{=3H7NmYfKn6}Select a clan to choose from their heroes.");
    }
}