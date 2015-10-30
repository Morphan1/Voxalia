using Voxalia.Shared;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.CommandSystem
{
    class ServerOutputter : Outputter
    {
        public Server TheServer;

        public ServerOutputter(Server tserver)
        {
            TheServer = tserver;
        }

        public override void WriteLine(string text)
        {
            SysConsole.WriteLine(text);
        }

        public override void Good(string tagged_text, DebugMode mode)
        {
            string text = TheServer.Commands.CommandSystem.TagSystem.ParseTagsFromText(tagged_text, TextStyle.Color_Outgood, null, mode);
            SysConsole.Output(OutputType.INFO, TextStyle.Color_Outgood + text);
        }

        public override void Bad(string tagged_text, DebugMode mode)
        {
            string text = TheServer.Commands.CommandSystem.TagSystem.ParseTagsFromText(tagged_text, TextStyle.Color_Outbad, null, mode);
            SysConsole.Output(OutputType.WARNING, TextStyle.Color_Outbad + text);
        }

        public override void UnknownCommand(CommandQueue queue, string basecommand, string[] arguments)
        {
            WriteLine(TextStyle.Color_Error + "Unknown command '" +
                TextStyle.Color_Standout + basecommand + TextStyle.Color_Error + "'.");
            if (queue.Outputsystem != null)
            {
                queue.Outputsystem.Invoke("Unknown command '" + TextStyle.Color_Standout
                    + basecommand + TextStyle.Color_Error + "'.", MessageType.BAD);
            }
        }

        public override string ReadTextFile(string name)
        {
            return Program.Files.ReadText("scripts/" + name);
        }
    }
}
