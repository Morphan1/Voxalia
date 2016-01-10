using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace VoxaliaServerSamplePlugin.SampleCommands
{
    class GreetingCommand: AbstractCommand
    {
        public SamplePlugin ThePlugin;

        public GreetingCommand(SamplePlugin plugin)
        {
            ThePlugin = plugin;
            Name = "greeting";
            Description = "Greets the server.";
            Arguments = "<message>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            entry.Good("'<{color.emphasis}>" + TagParser.Escape(entry.GetArgument(0)) + "<{color.base}>' to you as well from " + ThePlugin.Name + "!");
        }
    }
}
