using Battlemage.PlayerController.Data;
using Unity.Entities;
using UnityEngine;

namespace Battlemage.PlayerController.Authoring
{
    [DisallowMultipleComponent]
    public class PlayerControllerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Data.PlayerController());
                AddComponent<PlayerCharacterInputs>(entity);
            }
        }
    }
}