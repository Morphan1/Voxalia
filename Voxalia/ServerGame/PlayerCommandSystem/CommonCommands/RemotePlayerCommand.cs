using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.Shared;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    public class RemotePlayerCommand : AbstractPlayerCommand
    {
        public RemotePlayerCommand()
        {
            Name = "remote";
            Silent = false;
            // TODO: Required permission.
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count <= 0)
            {
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "/remote <commands>");
                return;
            }
            CommandQueue queue = CommandScript.SeparateCommands("command_line", entry.AllArguments(),
                entry.Player.TheServer.Commands.CommandSystem, false).ToQueue(entry.Player.TheServer.Commands.CommandSystem);
            queue.SetVariable("player", new PlayerTag(entry.Player));
            queue.Outputsystem = (message, messageType) =>
            {
                string bcolor = "^r^7";
                switch (messageType)
                {
                    case MessageType.INFO:
                    case MessageType.GOOD:
                        bcolor = "^r^2";
                        break;
                    case MessageType.BAD:
                        bcolor = "^r^3";
                        break;
                }
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, entry.Player.TheServer.Commands.CommandSystem.TagSystem.ParseTagsFromText(message, bcolor,
                    queue.CommandStack.Peek().Variables, DebugMode.FULL, (o) => { /* DO NOTHING */ }, true));
            };
            queue.Execute();
        }
    }
}
