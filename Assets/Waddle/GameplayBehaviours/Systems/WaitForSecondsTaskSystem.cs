using Unity.Entities;
using Waddle.EntitiesExtended.Extensions;
using Waddle.GameplayBehaviours.Data;

namespace Waddle.GameplayBehaviours.Systems
{
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct WaitForSecondsTaskSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaitForSecondsBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var secondsBuffer = state.EntityManager.GetSingletonManaged<WaitForSecondsBuffer>();
            var seconds = secondsBuffer.Seconds;
            for (var i = 0; i < seconds.Count; i++)
            {
                var wait = seconds[i];
                wait.Seconds -= SystemAPI.Time.DeltaTime;
                seconds[i] = wait;
                if (wait.Seconds <= 0)
                {
                    wait.Continuation();
                    seconds.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}