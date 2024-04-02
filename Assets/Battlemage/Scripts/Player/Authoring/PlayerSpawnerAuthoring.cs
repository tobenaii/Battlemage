using Unity.Entities;
using UnityEngine;

namespace Battlemage.Player.Authoring
{
    public struct PlayerSpawner : IComponentData
    {
        public Entity Character;
        public Entity Player;
    }

    [DisallowMultipleComponent]
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _characterPrefab;
        [SerializeField] private GameObject _playerPrefab;

        private class Baker : Baker<PlayerSpawnerAuthoring>
        {
            public override void Bake(PlayerSpawnerAuthoring authoring)
            {
                PlayerSpawner playerSpawner = default(PlayerSpawner);
                playerSpawner.Character = GetEntity(authoring._characterPrefab, TransformUsageFlags.Dynamic);
                playerSpawner.Player = GetEntity(authoring._playerPrefab, TransformUsageFlags.None);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, playerSpawner);
            }
        }
    }
}