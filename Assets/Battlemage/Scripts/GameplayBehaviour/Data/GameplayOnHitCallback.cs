using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public struct GameplayOnHitCallback : IComponentData
    {
        public delegate void Delegate(ref SystemState state, ref Entity ability, ref Entity target);

        public long Callback;
    }
}