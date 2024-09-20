using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Runtime.GameplayBehaviours
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventsSystemGroup : ComponentSystemGroup
    {
    }
}