using System;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.Scripting;

namespace Waddle.GameplayBehaviours.Data
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class GameplayEventDefinitionAttribute : Attribute
    {
        public Type DelegateType { get; }
        public GameplayEventDefinitionAttribute(Type delegateType)
        {
            DelegateType = delegateType;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class GameplayEventAttribute : PreserveAttribute
    {
        public ComponentType GameplayEventType { get; }
        
        public GameplayEventAttribute(Type gameplayEventType)
        {
            GameplayEventType = gameplayEventType;
        }
    }
}