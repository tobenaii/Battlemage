﻿using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Extensions
{
    public static class GameplayStateExtensions
    {
        public static void ScheduleEvent(this GameplayState gameplayState, Entity entity, float time, FixedString64Bytes scheduledEvent)
        {
            gameplayState.GetBuffer<GameplayScheduledEvent>(entity).Add(new GameplayScheduledEvent()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayScheduledEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
                Time = time,
                TimeStarted = gameplayState.Time.ElapsedTime
            });
        }
        
        public static void AddOverlapSphereCallback(this GameplayState gameplayState, Entity entity, float radius, FixedString64Bytes scheduledEvent)
        {
            gameplayState.GetBuffer<GameplayOnHitEvent>(entity).Add(new GameplayOnHitEvent()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayOnHitEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
                Radius = radius
            });
        }
    }
}