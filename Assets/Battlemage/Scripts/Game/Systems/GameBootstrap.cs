#if UNITY_EDITOR
using ParrelSync;
#endif
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
            if (!ClonesManager.IsClone())
            {
#endif
                var serverWorld = CreateServerWorld("Server World");
                var tickRate = new ClientServerTickRate();
                tickRate.ResolveDefaults();
#if UNITY_EDITOR
                tickRate.SimulationTickRate = 60;
                tickRate.NetworkTickRate = 30;
                tickRate.MaxSimulationStepsPerFrame = 3;
                tickRate.PredictedFixedStepSimulationTickRatio = 1;
                serverWorld.EntityManager.CreateSingleton(tickRate);
#else 
                tickRate.SimulationTickRate = 60;
                tickRate.NetworkTickRate = 60;
                tickRate.MaxSimulationStepsPerFrame = 3;
                tickRate.PredictedFixedStepSimulationTickRatio = 1;
#endif
#if UNITY_EDITOR
            }
#endif
            CreateClientWorld("Client World");

            return true;
        }
    }
}