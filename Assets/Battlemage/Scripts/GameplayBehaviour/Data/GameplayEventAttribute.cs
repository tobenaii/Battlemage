﻿using System;
using Unity.Burst;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class GameplayEventDefinitionAttribute : BurstCompileAttribute
    {
        public Type DelegateType { get; }
        public GameplayEventDefinitionAttribute(Type delegateType)
        {
            DelegateType = delegateType;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class GameplayEventAttribute : Attribute
    {
        public ComponentType GameplayEventType { get; }
        
        public GameplayEventAttribute(Type gameplayEventType)
        {
            GameplayEventType = gameplayEventType;
        }
    }
}