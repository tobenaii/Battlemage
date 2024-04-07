using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using Unity.Transforms;

namespace Battlemage.Utilities
{
    public static class MiscUtilities
    {
        public static void SetShadowModeInHierarchy(EntityManager entityManager, EntityCommandBuffer ecb,
            Entity onEntity, ref BufferLookup<Child> childBufferFromEntity,
            UnityEngine.Rendering.ShadowCastingMode mode)
        {
            if (entityManager.HasComponent<RenderFilterSettings>(onEntity))
            {
                RenderFilterSettings renderFilterSettings =
                    entityManager.GetSharedComponent<RenderFilterSettings>(onEntity);
                renderFilterSettings.ShadowCastingMode = mode;
                ecb.SetSharedComponent(onEntity, renderFilterSettings);
            }

            if (childBufferFromEntity.HasBuffer(onEntity))
            {
                DynamicBuffer<Child> childBuffer = childBufferFromEntity[onEntity];
                for (int i = 0; i < childBuffer.Length; i++)
                {
                    SetShadowModeInHierarchy(entityManager, ecb, childBuffer[i].Value, ref childBufferFromEntity, mode);
                }
            }
        }

        public static void DisableRenderingInHierarchy(EntityCommandBuffer ecb, Entity onEntity,
            ref BufferLookup<Child> childBufferFromEntity)
        {
            ecb.RemoveComponent<MaterialMeshInfo>(onEntity);

            if (childBufferFromEntity.HasBuffer(onEntity))
            {
                DynamicBuffer<Child> childBuffer = childBufferFromEntity[onEntity];
                for (int i = 0; i < childBuffer.Length; i++)
                {
                    DisableRenderingInHierarchy(ecb, childBuffer[i].Value, ref childBufferFromEntity);
                }
            }
        }

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
        
        public static Type GetDelegateType(MethodInfo methodInfo)
        {
            Type[] parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            Type returnType = methodInfo.ReturnType;
            return Expression.GetDelegateType(parameterTypes.Concat(new[] { returnType }).ToArray());
        }
    }
}