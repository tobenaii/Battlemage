using Unity.Collections;
using Unity.Entities;

namespace Waddle.Runtime.EntitiesExtended
{
    public static class EntityManagerExtensions
    {
        private const EntityQueryOptions QueryOptions = EntityQueryOptions.IncludeSystems;
        
        public static T GetSingleton<T>(this EntityManager em, bool completeDependency = true)
            where T : unmanaged, IComponentData
        {
            using var query = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().WithOptions(QueryOptions).Build(em);
            if (completeDependency)
            {
                query.CompleteDependency();
            }

            return query.GetSingleton<T>();
        }
        
        public static T GetSingletonManaged<T>(this EntityManager em, bool completeDependency = true)
            where T : class, IComponentData, new()
        {
            using var query = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().WithOptions(QueryOptions).Build(em);
            if (completeDependency)
            {
                query.CompleteDependency();
            }

            return em.GetComponentData<T>(query.GetSingletonEntity());
        }

        public static void SetSingleton<T>(this EntityManager em, T value, bool completeDependency = true)
            where T : unmanaged, IComponentData
        {
            using var query = new EntityQueryBuilder(Allocator.Temp).WithAllRW<T>().WithOptions(QueryOptions).Build(em);
            if (completeDependency)
            {
                query.CompleteDependency();
            }

            query.SetSingleton(value);
        }
        
        public static DynamicBuffer<T> GetSingletonBuffer<T>(this EntityManager em, bool isReadOnly = false)
            where T : unmanaged, IBufferElementData
        {
            using var query = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().WithOptions(QueryOptions).Build(em);

            return query.GetSingletonBuffer<T>(isReadOnly);
        }
    }
}