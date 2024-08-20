using Unity.Entities;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Extensions;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnHitEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref Entity target);
        public BlobAssetReference<GameplayEventPointer> EventBlob;
        public float Radius;
        
        public static void AddOnHitCallback(GameplayState gameplayState, Entity source, float radius, Delegate callback)
        {
            var buffer = gameplayState.GetBuffer<GameplayOnHitEvent>(source);
            var gameplayEventRefs = gameplayState.GetBuffer<GameplayEventReference>(source);
            var eventBlob = gameplayEventRefs.GetEventPointerBlob(TypeManager.GetTypeInfo<GameplayOnHitEvent>().StableTypeHash, callback.Method.Name);
            buffer.Add(new GameplayOnHitEvent
            {
                EventBlob = eventBlob,
                Radius = radius
            });
        }
    }
}