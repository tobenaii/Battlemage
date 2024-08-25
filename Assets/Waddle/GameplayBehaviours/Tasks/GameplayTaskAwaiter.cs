using System;
using System.Runtime.CompilerServices;
using Waddle.GameplayBehaviours.Data;
using WaitForSeconds = Waddle.GameplayBehaviours.Data.WaitForSeconds;

namespace Waddle.GameplayTasks
{
    public struct GameplayTaskAwaiter : INotifyCompletion
    {
        private GameplayState _gameplayState;
        private readonly float _seconds;
        
        public bool IsCompleted => false;

        public GameplayTaskAwaiter(GameplayState gameplayState, float seconds)
        {
            _gameplayState = gameplayState;
            _seconds = seconds;
        }
        
        public void OnCompleted(Action continuation)
        {
            _gameplayState.GetSingletonManaged<WaitForSecondsBuffer>().Seconds.Add(new WaitForSeconds()
            {
                Continuation = continuation,
                Seconds = _seconds
            });
        }

        public void GetResult()
        {
        }
    }
}