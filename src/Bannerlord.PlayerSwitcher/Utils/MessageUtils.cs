using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher.Utils
{
    internal static class MessageUtils
    {
        internal static void DisplayNotification(TextObject textObj, BasicCharacterObject character)
        {
            MBInformationManager.AddQuickInformation(textObj, announcerCharacter: character);
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