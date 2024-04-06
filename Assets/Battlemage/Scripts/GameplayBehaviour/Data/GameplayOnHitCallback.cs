using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public struct GameplayOnHitCallback : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref Entity target);

        public long Callback;
    }
}