using Unity.Entities;
using Unity.NetCode;
using Waddle.GameplayBehaviour.Systems;

namespace Waddle.GameplayActions.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayActionRequestsSystemGroup : ComponentSystemGroup
    {
    }
}