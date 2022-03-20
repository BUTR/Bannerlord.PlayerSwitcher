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
    }
}