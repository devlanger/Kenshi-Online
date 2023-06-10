namespace DefaultNamespace
{
    public abstract class GameMode
    {
        public virtual GameType GameType { get; }
        protected GameModeController _gameModeController;
        
        public virtual void Initialize(GameModeController gameModeController)
        {
            _gameModeController = gameModeController;
        }
    }

    public enum GameType
    {
        DEATHMATCH = 1,
        TEAM_DEATHMATCH = 2,
    }
}