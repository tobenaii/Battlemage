using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BovineLabs.Core.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviour.Data;

namespace Waddle.GameplayBehaviour.Utilities
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
                (uint)GetCleanMethodName(method).GetHashCode(), 0, 0);
        }
        
        public static Hash128 GetEventHash(Type gameplayBehaviour, ComponentType eventType, MethodInfo method)
        {
            return new Hash128(
                (uint)gameplayBehaviour.GetHashCode(),
                (uint)eventType.TypeIndex.GetHashCode(),
                (uint)GetCleanMethodName(method).GetHashCode(), 0);
        }

        private static string GetCleanMethodName(MethodInfo method)
        {
            return method.Name.Replace("$BurstManaged", "");
        }

        public static BlobAssetReference<EventPointer> CreateEventPointerBlob(Delegate eventDelegate, bool burstCompiled)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var eventPointer = ref builder.ConstructRoot<EventPointer>();
            if (burstCompiled)
            {
                eventPointer.Pointer = BurstCompiler.CompileFunctionPointer(eventDelegate).Value;
            }
            else
            {
                eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
            }
            var result = builder.CreateBlobAssetReference<EventPointer>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }
}