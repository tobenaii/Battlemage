using Unity.CharacterController;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battlemage.Character.Authoring
{
    [DisallowMultipleComponent]
    public class CharacterAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _viewObject;
        [SerializeField] private AuthoringKinematicCharacterProperties _characterProperties = AuthoringKinematicCharacterProperties.GetDefault();
        [SerializeField] private CharacterSettings _character = CharacterSettings.GetDefault();

        public class Baker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring._characterProperties);

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
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