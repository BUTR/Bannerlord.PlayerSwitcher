using Bannerlord.ButterLib.HotKeys;
using Bannerlord.PlayerSwitcher.CampaignBehaviors;
using Bannerlord.PlayerSwitcher.HotKeys;
using Bannerlord.PlayerSwitcher.Patches;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.PlayerSwitcher
{
    public sealed class SubModule : MBSubModuleBase
    {
        private readonly Harmony _harmony = new("Bannerlord.PlayerSwitcher");

        private bool _isInitialized;

        protected override void OnSubModuleLoad()
        {
            ClanPatch.Enable(_harmony);
            ChangeClanLeaderActionPatch.Enable(_harmony);
            SPInventoryVMPatch.Enable(_harmony);

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
                cgs.AddBehavior(new StorageCampaignBehavior());
            }

            base.OnGameStart(game, gameStarterObject);
        }
    }
}