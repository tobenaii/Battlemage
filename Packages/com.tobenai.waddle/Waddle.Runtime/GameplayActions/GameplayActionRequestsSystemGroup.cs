using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Runtime.GameplayActions
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayActionRequestsSystemGroup : ComponentSystemGroup
    {
    }
}