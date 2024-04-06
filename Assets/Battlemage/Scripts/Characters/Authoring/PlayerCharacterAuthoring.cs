using Battlemage.GameplayBehaviour.Authoring;
using Unity.Entities;

namespace Battlemage.Characters.Authoring
{
    public class PlayerCharacterAuthoring : GameplayBehaviourAuthoring
    {
        public class PlayerCharacterAuthoringBaker : Baker<PlayerCharacterAuthoring>
        {
            public override void Bake(PlayerCharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }
}