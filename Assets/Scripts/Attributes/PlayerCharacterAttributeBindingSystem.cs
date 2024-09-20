using Battlemage.Player;
using Unity.Entities;
using Waddle.Runtime.GameplayAttributes;

namespace Battlemage.Attributes
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
            var attributes = SystemAPI.GetBuffer<GameplayAttribute>(localCharacter);
            foreach (var attributeBinding in attributeBindings)
            {
                var attributeValue = attributes[attributeBinding.Attribute];
                attributeBinding.Binding.Value.Value = attributeValue.CurrentValue;
            }
        }
    }
}