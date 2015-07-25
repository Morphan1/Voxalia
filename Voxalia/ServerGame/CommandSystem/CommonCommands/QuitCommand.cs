using Frenetic.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.CommandSystem.CommonCommands
{
    public class QuitCommand: AbstractCommand
    {
        public Server TheServer;

        public QuitCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "quit";
            Description = "Closes the server entirely.";
            Arguments = "";
        }

        public override void Execute(CommandEntry entry)
        {
            SysConsole.Output(OutputType.INFO, "[Shutdown] Starting to close server...");
            foreach (World world in TheServer.LoadedWorlds)
            {
                foreach (PlayerEntity player in world.Players)
                {
                    player.Kick("Server shutting down.");
                }
                SysConsole.Output(OutputType.INFO, "[Shutdown] Unloading world: " + world.Name);
                world.UnloadFully();
            }
            SysConsole.Output(OutputType.INFO, "[Shutdown] Closing server...");
            TheServer.ShutDownQuickly();
        }
    }
}
