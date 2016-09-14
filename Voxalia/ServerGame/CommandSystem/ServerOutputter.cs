using System;
using Voxalia.Shared;
using FreneticScript;
using FreneticScript.CommandSystem;
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
            // TODO: Change maybe?
            SysConsole.WriteLine(text, "^r^7");
        }

        public override void Good(string tagged_text, DebugMode mode)
        {
            string text = TheServer.Commands.CommandSystem.TagSystem.ParseTagsFromText(tagged_text, TextStyle.Color_Outgood, null, mode, (o) => { throw new Exception("Tag exception: " + o); }, true);
            SysConsole.Output(OutputType.INFO, TextStyle.Color_Outgood + text);
        }

        public override void Bad(string tagged_text, DebugMode mode)
        {
            string text = TheServer.Commands.CommandSystem.TagSystem.ParseTagsFromText(tagged_text, TextStyle.Color_Outbad, null, mode, (o) => { throw new Exception("Tag exception: " + o); }, true);
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
            return TheServer.Files.ReadText("scripts/server/" + name);
        }

        public override byte[] ReadDataFile(string name)
        {
            return TheServer.Files.ReadBytes("script_data/server/" + name);
        }

        public override void WriteDataFile(string name, byte[] data)
        {
            TheServer.Files.WriteBytes("script_data/server/" + name, data);
        }

        public override void Reload()
        {
            TheServer.Recipes.Recipes.Clear();
            TheServer.AutorunScripts();
        }

        public override bool ShouldErrorOnInvalidCommand()
        {
            return true;
        }
    }
}
