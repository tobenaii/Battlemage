using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Waddle.GameplayLifecycle.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial class InstantiateCommandBufferSystem : EntityCommandBufferSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            this.RegisterSingleton<Singleton>(ref this.PendingBuffers, this.World.Unmanaged);
        }
        
        public unsafe struct Singleton : IComponentData, IECBSingleton
        {
            private UnsafeList<EntityCommandBuffer>* pendingBuffers;
            private AllocatorManager.AllocatorHandle allocator;
            
            public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
            {
                return EntityCommandBufferSystem.CreateCommandBuffer(ref *this.pendingBuffers, this.allocator, world);
            }
            
            void IECBSingleton.SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
            {
                this.pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
            }
            
            void IECBSingleton.SetAllocator(Allocator allocatorIn)
            {
                this.allocator = allocatorIn;
            }
            
            void IECBSingleton.SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
            {
                this.allocator = allocatorIn;
            }
        }
    }
}