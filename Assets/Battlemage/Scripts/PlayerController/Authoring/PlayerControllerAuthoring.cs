using Unity.Entities;
using UnityEngine;

namespace Battlemage.PlayerController.Authoring
{
    [DisallowMultipleComponent]
    public class PlayerControllerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _controlledCharacter;

        public class Baker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerController
                {
                    Value = GetEntity(authoring._controlledCharacter, TransformUsageFlags.Dynamic),
                });
                AddComponent<PlayerCharacterInputs>(entity);
            }
        }
    }
}