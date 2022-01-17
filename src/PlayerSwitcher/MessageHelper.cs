using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace PlayerSwitcher
{
    internal static class MessageHelper
    {
        internal static void DisplayNotification(string msg, BasicCharacterObject character)
        {
            InformationManager.AddQuickInformation(new TextObject(msg), announcerCharacter: character);
        }

        internal static void DisplayNotification(TextObject textObj, BasicCharacterObject character)
        {
            InformationManager.AddQuickInformation(textObj, announcerCharacter: character);
        }

        internal static void DisplayMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg));
        }

        internal static void DisplayMessage(TextObject textObj)
        {
            InformationManager.DisplayMessage(new InformationMessage(textObj.ToString()));
        }

        internal static void DisplayMessage(string msg, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg, color));
        }

        internal static void DisplayMessage(TextObject textObj, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(textObj.ToString(), color));
        }

#if DEBUG
        internal static void DisplayDebugMessage(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage("[PlayerSwitcher DEBUG] " + msg, Colors.Cyan));
        }

        internal static void DisplayDebugMessage(string msg, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage("[PlayerSwitcher DEBUG] " + msg, color));
        }
#endif
    }
}