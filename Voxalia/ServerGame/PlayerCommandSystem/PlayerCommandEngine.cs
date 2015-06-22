using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.ServerGame.PlayerCommandSystem.CommonCommands;

namespace Voxalia.ServerGame.PlayerCommandSystem
{
    public class PlayerCommandEngine
    {
        Dictionary<string, AbstractPlayerCommand> Commands = new Dictionary<string, AbstractPlayerCommand>();

        public PlayerCommandEngine()
        {
            Register(new DevelPlayerCommand());
            Register(new DropPlayerCommand());
            Register(new ReloadPlayerCommand());
            Register(new SayPlayerCommand());
            Register(new ThrowPlayerCommand());
            Register(new StancePlayerCommand());
            Register(new UsePlayerCommand());
        }

        public void Register(AbstractPlayerCommand cmd)
        {
            Commands.Add(cmd.Name, cmd);
        }

        public void Execute(PlayerEntity entity, List<string> arguments, string commandname)
        {
            PlayerCommandEntry entry = new PlayerCommandEntry();
            entry.Player = entity;
            entry.InputArguments = arguments;
            entry.Command = GetCommand(commandname);
            if (entry.Command == null || !entry.Command.Silent)
            {
                StringBuilder args = new StringBuilder();
                for (int i = 0; i < arguments.Count; i++)
                {
                    args.Append(" \"").Append(arguments[i]).Append("\"");
                }
                SysConsole.Output(OutputType.INFO, "Client " + entity + " executing command '" + commandname + "' with arguments:" + args.ToString());
            }
            // TODO: Permission
            // TODO: Fire command event
            if (entry.Command == null)
            {
                entry.Player.Network.SendMessage("Unknown command."); // TODO: Noise mode // TODO: Language
            }
            else
            {
                entry.Command.Execute(entry);
            }
        }

        public AbstractPlayerCommand GetCommand(string name)
        {
            AbstractPlayerCommand apc;
            Commands.TryGetValue(name.ToLower(), out apc);
            return apc;
        }
    }
}
