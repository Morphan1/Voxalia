using FreneticScript.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.CommandSystem.PlayerCommands
{
    class KickCommand: AbstractCommand
    {
        public Server TheServer;

        public KickCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "kick";
            Description = "Kicks player(s) from the server.";
            Arguments = "<player list> [message]";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            ListTag list = ListTag.For(entry.GetArgument(0));
            string message = "Kicked by the server.";
            if (entry.Arguments.Count >= 2)
            {
                message = "Kicked by the server: " + entry.GetArgument(1);
            }
            for (int i = 0; i < list.ListEntries.Count; i++)
            {
                PlayerEntity pl = TheServer.GetPlayerFor(list.ListEntries[i].ToString());
                if (pl == null)
                {
                    entry.Bad("Unknown player " + TagParser.Escape(list.ListEntries[i].ToString()));
                }
                else
                {
                    pl.Kick(message);
                }
            }
        }
    }
}
