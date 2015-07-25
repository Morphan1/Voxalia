using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    public class ReloadCommand: AbstractCommand
    {
        public Client TheClient;

        public ReloadCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "reload";
            Description = "Reloads all or part of the game";
            Arguments = "<chunks/all>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string arg = entry.GetArgument(0).ToLower();
            bool success = false;
            if (arg == "chunks" || arg == "all")
            {
                success = true;
                foreach (Chunk chunk in TheClient.TheWorld.LoadedChunks.Values)
                {
                    chunk.AddToWorld();
                    chunk.CreateVBO();
                }
            }
            if (!success)
            {
                entry.Bad("Invalid argument.");
            }
        }
    }
}
