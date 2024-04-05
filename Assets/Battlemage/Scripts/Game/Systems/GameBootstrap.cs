using System.Linq;
using Unity.Multiplayer.Playmode;
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

            var tags = CurrentPlayer.ReadOnlyTags();

            if (tags.Contains("Server"))
            {
                CreateServerWorld("Server World");
            }
            CreateClientWorld("Client World");

            return true;
        }
    }
}