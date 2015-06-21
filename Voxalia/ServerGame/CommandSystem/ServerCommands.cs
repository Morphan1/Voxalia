using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.ServerGame.CommandSystem.CommonCommands;
using Voxalia.ServerGame.CommandSystem.MapCommands;
using Voxalia.ServerGame.CommandSystem.PlayerCommands;

namespace Voxalia.ServerGame.CommandSystem
{
    /// <summary>
    /// Handles all console commands and key binds.
    /// </summary>
    public class ServerCommands
    {
        /// <summary>
        /// The client that manages this command system.
        /// </summary>
        public Server TheServer;

        /// <summary>
        /// The Commands object that all commands actually go to.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// The output system.
        /// </summary>
        public Outputter Output;

        /// <summary>
        /// Prepares the command system, registering all base commands.
        /// </summary>
        public void Init(Outputter _output, Server tserver)
        {
            // General Init
            TheServer = tserver;
            CommandSystem = new Commands();
            Output = _output;
            CommandSystem.Output = Output;
            CommandSystem.Init();

            // Common Commands
            CommandSystem.RegisterCommand(new SayCommand(TheServer));

            // Map Commands
            CommandSystem.RegisterCommand(new LoadCommand(TheServer));
            
            // Player Management Commands
            CommandSystem.RegisterCommand(new KickCommand(TheServer));
        }

        /// <summary>
        /// Advances any running command queues.
        /// </summary>
        public void Tick()
        {
            CommandSystem.Tick((float)TheServer.Delta);
        }

        /// <summary>
        /// Executes an arbitrary list of command inputs (separated by newlines, semicolons, ...)
        /// </summary>
        /// <param name="commands">The command string to parse</param>
        public void ExecuteCommands(string commands)
        {
            CommandSystem.ExecuteCommands(commands, null);
        }
    }
}
