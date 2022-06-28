using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.PlayerSwitcher
{
    internal static class MessageUtils
    {
        internal static void DisplayNotification(TextObject textObj, BasicCharacterObject character)
        {
#if e172
            InformationManager.AddQuickInformation(textObj, announcerCharacter: character);
#elif e180
            MBInformationManager.AddQuickInformation(textObj, announcerCharacter: character);
#endif
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