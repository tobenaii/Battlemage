using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Waddle.GameplayBehaviour.Data;
using Hash128 = Unity.Entities.Hash128;

namespace Waddle.GameplayBehaviour.Authoring
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial class GameplayEventsBakingSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<GameplayEventInfoElement>();
        }

        protected override void OnUpdate()
        {
            var blobMappings = SystemAPI.GetSingletonBuffer<GameplayEventInfoElement>();
            var mappingHashes = new List<Hash128>();
            foreach (var blobMapping in SystemAPI
                         .Query<GameplayEventInfo>()
                         .WithOptions(EntityQueryOptions.IncludePrefab))
            {
                var hash = new Hash128(
                    (uint)blobMapping.AssemblyQualifiedName.ToString().GetHashCode(), 
                    (uint)blobMapping.MethodName.ToString().GetHashCode(), 0, 0);
                if (!mappingHashes.Contains(hash))
                {
                    mappingHashes.Add(hash);
                    blobMappings.Add(new GameplayEventInfoElement { Info = blobMapping });
                }
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (fullEventRefs, entity) in
                     SystemAPI
                         .Query<DynamicBuffer<FullGameplayEventReference>>()
                         .WithEntityAccess()
                         .WithOptions(EntityQueryOptions.IncludePrefab))
            {
                var eventRefs = ecb.AddBuffer<GameplayEventReference>(entity);
                foreach (var fullEventRef in fullEventRefs)
                {
                    int eventIndex = mappingHashes.IndexOf(fullEventRef.FullHash);
                    eventRefs.Add(new GameplayEventReference()
                    {
                        TypeHash = fullEventRef.TypeHash,
                        MethodHash = fullEventRef.MethodHash,
                        Index = eventIndex
                    });
                }
                ecb.RemoveComponent<FullGameplayEventReference>(entity);
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}