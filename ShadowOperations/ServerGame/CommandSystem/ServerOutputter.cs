using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.CommandSystem
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
            string text = TheServer.Commands.CommandSystem.TagSystem.ParseTags(tagged_text, TextStyle.Color_Outgood, null, mode);
            SysConsole.Output(OutputType.INFO, TextStyle.Color_Outgood + text);
        }

        public override void Bad(string tagged_text, DebugMode mode)
        {
            string text = TheServer.Commands.CommandSystem.TagSystem.ParseTags(tagged_text, TextStyle.Color_Outbad, null, mode);
            SysConsole.Output(OutputType.WARNING, TextStyle.Color_Outbad + text);
        }

        public override void UnknownCommand(string basecommand, string[] arguments)
        {
            WriteLine(TextStyle.Color_Error + "Unknown command '" +
                TextStyle.Color_Standout + basecommand + TextStyle.Color_Error + "'.");
        }
    }
}
