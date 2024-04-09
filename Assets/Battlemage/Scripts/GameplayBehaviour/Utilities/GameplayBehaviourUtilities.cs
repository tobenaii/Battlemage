using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using BovineLabs.Core.Extensions;
using Unity.Collections;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Utilities
{
    public static class GameplayBehaviourUtilities
    {
        public static BlobAssetReference<EventPointer> FindEventPointerByHash(EntityManager entityManager, Hash128 hash)
        {
            var entities = entityManager.CreateEntityQuery(ComponentType.ReadOnly<GameplayEventBlobMapping>())
                .ToEntityArray(Allocator.Temp);
            var blobMappings = entityManager.GetComponentLookup<GameplayEventBlobMapping>();
            BlobAssetReference<EventPointer> eventPointerReference = default;
            foreach (var entity in entities)
            {
                var blobMapping = blobMappings[entity];
                if (blobMapping.Hash == hash)
                {
                    eventPointerReference = blobMapping.Pointer;
                    break;
                }
            }

            entities.Dispose();
            return eventPointerReference;
        }
        
        public static Hash128 GetEventHash(ComponentType eventType)
        {
            return new Hash128(
                (uint)eventType.TypeIndex.GetHashCode(), 0, 0, 0);
        }
        
        public static Hash128 GetEventHash(ComponentType eventType, MethodInfo method)
        {
            return new Hash128(
                (uint)eventType.TypeIndex.GetHashCode(), 
                (uint)method.Name.GetHashCode(), 0, 0);
        }
        
        public static Hash128 GetEventHash(Type gameplayBehaviour, ComponentType eventType, MethodInfo method)
        {
            return new Hash128(
                (uint)gameplayBehaviour.GetHashCode(),
                (uint)eventType.TypeIndex.GetHashCode(),
                (uint)method.Name.GetHashCode(), 0);
        }

        public static BlobAssetReference<EventPointer> CreateEventPointerBlob(Delegate eventDelegate)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var eventPointer = ref builder.ConstructRoot<EventPointer>();
            eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
            var result = builder.CreateBlobAssetReference<EventPointer>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}