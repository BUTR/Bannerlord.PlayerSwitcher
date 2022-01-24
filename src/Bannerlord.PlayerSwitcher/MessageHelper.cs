using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher
{
    internal static class MessageHelper
    {
        internal static void DisplayNotification(TextObject textObj, BasicCharacterObject character)
        {
            InformationManager.AddQuickInformation(textObj, announcerCharacter: character);
        }

        internal static void DisplayMessage(TextObject textObj)
        {
            InformationManager.DisplayMessage(new InformationMessage(textObj.ToString()));
        }

        internal static void DisplayMessage(TextObject textObj, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage(textObj.ToString(), color));
        }

#if DEBUG
        internal static void DisplayDebugMessage(TextObject textObj)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[PlayerSwitcher DEBUG] {textObj}", Colors.Cyan));
        }

        internal static void DisplayDebugMessage(TextObject textObj, Color color)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[PlayerSwitcher DEBUG] {textObj}", color));
        }
#endif
    }
}