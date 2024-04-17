using ParrelSync;
using Unity.NetCode;

// Create a custom bootstrap, which enables auto-connect.
// The bootstrap can also be used to configure other settings as well as to
// manually decide which worlds (client and server) to create based on user input
namespace Battlemage.Game.Systems
{
    [UnityEngine.Scripting.Preserve]
    public class GameBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7979; // Enabled auto connect
            
            if (!ClonesManager.IsClone())
            {
                var serverWorld = CreateServerWorld("Server World");
                var tickRate = new ClientServerTickRate();
                tickRate.ResolveDefaults();
                tickRate.SimulationTickRate = 60;
                tickRate.NetworkTickRate = 30;
                tickRate.MaxSimulationStepsPerFrame = 3;
                tickRate.PredictedFixedStepSimulationTickRatio = 1;
                serverWorld.EntityManager.CreateSingleton(tickRate);
            }
            CreateClientWorld("Client World");

            return true;
        }
    }
}