using Unity.Collections;
using Unity.Entities;

namespace Battlemage.Utilities
{
    public static class MiscUtilities
    {
        public static bool HasSingleton<T>(ref SystemState state) where T : unmanaged, IComponentData
        {
            return new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(ref state).HasSingleton<T>();
        }

        public static T GetSingleton<T>(ref SystemState state) where T : unmanaged, IComponentData
        {
            return new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(ref state).GetSingleton<T>();
        }

        public static Entity GetSingletonEntity<T>(ref SystemState state) where T : unmanaged, IComponentData
        {
            return new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(ref state).GetSingletonEntity();
        }
    }
}