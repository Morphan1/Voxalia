//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.Collections.Generic;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.ServerGame.PlayerCommandSystem.CommonCommands;
using Voxalia.ServerGame.PlayerCommandSystem.RegionCommands;
using FreneticScript;

namespace Voxalia.ServerGame.PlayerCommandSystem
{
    public class PlayerCommandEngine
    {
        Dictionary<string, AbstractPlayerCommand> Commands = new Dictionary<string, AbstractPlayerCommand>();

        public PlayerCommandEngine()
        {
            // Common
            Register(new DevelPlayerCommand());
            Register(new DropPlayerCommand());
            Register(new RemotePlayerCommand());
            Register(new SayPlayerCommand());
            Register(new StancePlayerCommand());
            Register(new ThrowPlayerCommand());
            Register(new WeaponreloadPlayerCommand());
            // Region
            Register(new BlockfloodPlayerCommand());
            Register(new BlockshapePlayerCommand());
            Register(new BlockshipPlayerCommand());
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
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "Unknown command."); // TODO: Noise mode // TODO: Language
            }
            else
            {
                entry.Command.Execute(entry);
            }
        }

        public AbstractPlayerCommand GetCommand(string name)
        {
            AbstractPlayerCommand apc;
            Commands.TryGetValue(name.ToLowerFast(), out apc);
            return apc;
        }
    }
}
