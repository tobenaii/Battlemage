using Unity.Entities;
using Unity.NetCode;
using Waddle.GameplayBehaviours.Systems;

namespace Waddle.GameplayActions.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayActionRequestsSystemGroup : ComponentSystemGroup
    {
    }
}