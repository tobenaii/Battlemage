using Unity.Entities;

namespace Waddle.Runtime.EntitiesExtended
{
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class BeginSimulationSystemGroup : ComponentSystemGroup
    {
    }
}