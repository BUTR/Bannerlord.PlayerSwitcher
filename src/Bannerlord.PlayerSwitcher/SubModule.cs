using Bannerlord.ButterLib.HotKeys;
using Bannerlord.PlayerSwitcher.CampaignBehaviors;
using Bannerlord.PlayerSwitcher.HotKeys;
using Bannerlord.PlayerSwitcher.Patches;
using Bannerlord.PlayerSwitcher.ScreenLayers;

using HarmonyLib;

using SandBox.View.Map;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.PlayerSwitcher
{
    public sealed class SubModule : MBSubModuleBase
    {
        private readonly Harmony _harmony = new("Bannerlord.PlayerSwitcher");

        private bool _isInitialized;

        protected override void OnSubModuleLoad()
        {
            ChangeClanLeaderActionPatch.Enable(_harmony);
            HeirSelectionCampaignBehaviorPatch.Enable(_harmony);
            LordPartyComponentPatch.Enable(_harmony);
            AddRemoveCompanionActionPatch.Enable(_harmony);

            base.OnSubModuleLoad();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                if (HotKeyManager.Create("Bannerlord.PlayerSwitcher") is { } hkm)
                {
                    hkm.Add<SwitchKey>();
                    hkm.Build();
                }
            }

            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter cgs)
            {
                var storageCampaignBehavior = new StorageCampaignBehavior();
                cgs.AddBehavior(storageCampaignBehavior);

                // This way we can only add once the screen.
                // StoryModeGauntletUISubModule uses a similar technique, but with a global variable
                // We play it better
                void OnPushScreen(ScreenBase screen)
                {
                    if (screen is MapScreen mapScreen)
                    {
                        mapScreen.AddLayer(new SwitchManagerMapScreenLayer(storageCampaignBehavior));
                        ScreenManager.OnPushScreen -= OnPushScreen;
                    }
                }
                ScreenManager.OnPushScreen += OnPushScreen;
            }

            base.OnGameStart(game, gameStarterObject);
        }
    }
}