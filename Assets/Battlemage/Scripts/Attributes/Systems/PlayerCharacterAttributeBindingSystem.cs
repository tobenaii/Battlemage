using Battlemage.Player.Data;
using BovineLabs.Core.Iterators;
using Unity.Entities;
using Waddle.GameplayAttributes.Data;

namespace Battlemage.Attributes.Systems
{
    public struct PlayerCharacterAttributeBinding : IBufferElementData
    {
        public byte Attribute;
        public UnityObjectRef<GameplayAttributeBindingObject> Binding;
    }
    
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class PlayerCharacterAttributeBindingSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<PlayerCharacterTag>();
            RequireForUpdate<PlayerCharacterAttributeBinding>();
        }

        protected override void OnUpdate()
        {
            var attributeBindings = SystemAPI.GetSingletonBuffer<PlayerCharacterAttributeBinding>(true);
            var localCharacter = SystemAPI.GetSingletonEntity<PlayerCharacterTag>();
            var attributeMap = SystemAPI.GetBuffer<GameplayAttributeMap>(localCharacter).AsHashMap<GameplayAttributeMap, byte, GameplayAttribute>();
            foreach (var attributeBinding in attributeBindings)
            {
                if (attributeMap.TryGetValue(attributeBinding.Attribute, out var attributeValue))
                {
                    attributeBinding.Binding.Value.Value = attributeValue.CurrentValue;
                }
            }
        }
    }
}