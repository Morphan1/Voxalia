using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A quick command to quit the game.
    /// </summary>
    class QuitCommand: AbstractCommand
    {
        public Client TheClient;

        public QuitCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "quit";
            Description = "Quits the game.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            TheClient.Window.Close();
        }
    }
}
