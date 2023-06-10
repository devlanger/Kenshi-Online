using StarterAssets;

namespace DefaultNamespace
{
    public class DeathmatchMode : GameMode
    {
        public override GameType GameType => GameType.DEATHMATCH;

        public int deathCounter = 0;
        public int deathsToFinishGame = 1;
        
        public override void Initialize(GameModeController gameModeController)
        {
            base.Initialize(gameModeController);
            
            CombatController.Instance.OnPlayerDeath += InstanceOnOnPlayerDeath;
        }

        private void InstanceOnOnPlayerDeath(Player obj)
        {
            deathCounter++;

            if (deathCounter >= deathsToFinishGame)
            {
                _gameModeController.FinishGame();
            }
        }
    }
}