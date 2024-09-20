using Unity.Entities;
using Unity.NetCode;

namespace Waddle.GameplayActions.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayActionRequestsSystemGroup : ComponentSystemGroup
    {
    }
}