using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.NetCode;
using Waddle.GameplayActions.Systems;
using Waddle.GameplayBehaviours.Systems;

namespace Waddle.GameplayAbilities.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayEventsSystemGroup))]
    [UpdateBefore(typeof(GameplayActionRequestsSystemGroup))]
    public partial class AbilitySpawnEntityCommandBufferSystem : EntityCommandBufferSystem
    {
        public unsafe struct Singleton : IComponentData, IECBSingleton
        {
            private UnsafeList<EntityCommandBuffer>* _pendingBuffers;
            private Allocator _allocator;

            public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
            {
                return EntityCommandBufferSystem.CreateCommandBuffer(ref *_pendingBuffers, _allocator, world);
            }

            public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
            {
                _pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
            }

            public void SetAllocator(Allocator allocatorIn)
            {
                _allocator = allocatorIn; 
            }
        }
    
        protected override unsafe void OnCreate()
        {
            base.OnCreate();

            ref UnsafeList<EntityCommandBuffer> pendingBuffers = ref *m_PendingBuffers;
            this.RegisterSingleton<Singleton>(ref pendingBuffers, World.Unmanaged);
        }
    }
}