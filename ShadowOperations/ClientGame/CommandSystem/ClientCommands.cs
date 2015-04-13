using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem
{
    /// <summary>
    /// Handles all console commands and key binds.
    /// </summary>
    public class ClientCommands
    {
        /// <summary>
        /// The client that manages this command system.
        /// </summary>
        public Client TheClient;

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
        public void Init(Outputter _output, Client tclient)
        {
            // General Init
            TheClient = tclient;
            CommandSystem = new Commands();
            Output = _output;
            CommandSystem.Output = Output;
            CommandSystem.Init();

            // TODO: Register commands
        }

        /// <summary>
        /// Advances any running command queues.
        /// </summary>
        public void Tick()
        {
            CommandSystem.Tick((float)TheClient.Delta);
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
