using Unity.Collections;
using Unity.Entities;
using Waddle.Runtime.GameplayBehaviours;

namespace Battlemage.GameplayBehaviours.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnHitEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref Entity target);
        public BlobAssetReference<GameplayEventPointer> EventBlob;
        public float Radius;
        
        public static void AddOnHitCallback(GameplayState gameplayState, Entity source, float radius, FixedString32Bytes methodName)
        {
            var buffer = gameplayState.GetBuffer<GameplayOnHitEvent>(source);
            var gameplayEventRefs = gameplayState.GetBuffer<GameplayEventReference>(source);
            var eventBlob = gameplayEventRefs.GetEventPointerBlob(TypeManager.GetTypeInfo<GameplayOnHitEvent>().StableTypeHash, methodName);
            buffer.Add(new GameplayOnHitEvent
            {
                EventBlob = eventBlob,
                Radius = radius
            });
        }
    }
}