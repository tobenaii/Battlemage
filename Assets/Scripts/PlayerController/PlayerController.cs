using System;
using Unity.Entities;
using Unity.NetCode;

namespace Battlemage.PlayerController
{
    [Serializable]
    [GhostComponent]
    public struct PlayerController : IComponentData
    {
        [GhostField] public Entity Character;
    }
}