using System;
using System.Reflection;
using AOT;
using Unity.Entities;

namespace Waddle.Runtime.GameplayBehaviours
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
    public class GameplayEventAttribute : MonoPInvokeCallbackAttribute
    {
        public ComponentType GameplayEventType { get; }
        
        public GameplayEventAttribute(Type gameplayEventType) : base(gameplayEventType.GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType)
        {
            GameplayEventType = gameplayEventType;
        }
    }
}