using Unity.Collections;
using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    public struct InitializeGameplayEvents : IComponentData
    {
    }
    
    public struct GameplayEventInfoElement : IBufferElementData
    {
        public GameplayEventInfo Info;
    }
    
    public struct GameplayEventInfo : IComponentData
    {
        public FixedString512Bytes AssemblyQualifiedName;
        public FixedString64Bytes MethodName;
    }
}