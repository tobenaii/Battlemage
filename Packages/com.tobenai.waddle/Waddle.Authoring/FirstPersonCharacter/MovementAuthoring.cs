using Unity.CharacterController;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Waddle.Runtime.FirstPersonCharacter;

namespace Waddle.Authoring.FirstPersonCharacter
{
    [DisallowMultipleComponent]
    public class MovementAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _viewObject;
        [SerializeField] private AuthoringKinematicCharacterProperties _characterProperties = AuthoringKinematicCharacterProperties.GetDefault();
        [SerializeField] private CharacterSettings _character = CharacterSettings.GetDefault();

        public class Baker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring._characterProperties);

                var entity = GetEntity(TransformUsageFlags.Dynamic);
            
                authoring._character.ViewEntity = GetEntity(authoring._viewObject, TransformUsageFlags.Dynamic);
        
                AddComponent(entity, authoring._character);
                AddComponent(entity, new CharacterViewRotation());
                AddComponent(entity, new FirstPersonCharacterControl());
            }
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct CharacterViewSetupBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (characterSettings, entity) in SystemAPI.Query<RefRO<CharacterSettings>>().WithOptions(EntityQueryOptions.IncludePrefab).WithEntityAccess())
            {
                if (characterSettings.ValueRO.ViewEntity == Entity.Null) continue;
                if (!SystemAPI.HasComponent<CharacterView>(characterSettings.ValueRO.ViewEntity))
                {
                    ecb.AddComponent(characterSettings.ValueRO.ViewEntity, new CharacterView { CharacterEntity = entity });
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}