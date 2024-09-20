using Unity.Entities;
using Unity.NetCode;

namespace Waddle.GameplayBehaviours.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventsSystemGroup : ComponentSystemGroup
    {
    }
}