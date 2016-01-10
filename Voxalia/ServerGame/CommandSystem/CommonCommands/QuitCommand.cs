using FreneticScript.CommandSystem;
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
            TheServer.ShutDown();
        }
    }
}
