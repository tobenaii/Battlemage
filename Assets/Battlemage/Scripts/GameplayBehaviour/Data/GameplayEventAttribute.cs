using System;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;

namespace Battlemage.GameplayBehaviour.Data
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class GameplayEventDefinitionAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class GameplayEventAttribute : Attribute
    {
        public Type GameplayEventType { get; }
        
        public GameplayEventAttribute(Type gameplayEventType)
        {
            GameplayEventType = gameplayEventType;
        }
    }
}