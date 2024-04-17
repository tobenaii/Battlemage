using System;
using Unity.Entities;
using Unity.NetCode;

namespace Battlemage.PlayerController.Data
{
    [Serializable]
    [GhostComponent]
    public struct PlayerController : IComponentData
    {
        [GhostField] public Entity Character;
        public Entity PrimaryAbilityPrefab;
    }
}