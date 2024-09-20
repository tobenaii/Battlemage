#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif
using System.Linq;
using Unity.Entities;
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
            
            TypeManager.Initialize();
            
#if UNITY_EDITOR
            var mppmTags = CurrentPlayer.ReadOnlyTags();
            if (!mppmTags.Contains("Client"))
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
#endif
            CreateClientWorld("Client World");

            return true;
        }
    }
}