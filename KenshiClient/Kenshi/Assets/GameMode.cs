using Kenshi.Shared.Models;

namespace DefaultNamespace
{
    [System.Serializable]
    public abstract class GameMode
    {
        public virtual GameType GameType { get; }
        protected GameModeController _gameModeController;
        
        public virtual void Initialize(GameModeController gameModeController)
        {
            _gameModeController = gameModeController;
        }
    }
}