using Waddle.GameplayBehaviours.Data;

namespace Waddle.GameplayTasks
{
    public readonly struct GameplayTask
    {
        private readonly GameplayState _gameplayState;
        private readonly float _seconds;
        
        public GameplayTask(GameplayState gameplayState, float seconds)
        {
            _gameplayState = gameplayState;
            _seconds = seconds;
        }
        
        public GameplayTaskAwaiter GetAwaiter()
        {
            return new GameplayTaskAwaiter(_gameplayState, _seconds);
        }
    }
}