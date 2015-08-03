using Frenetic.CommandSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.ClientMainSystem;
using System.Drawing;
using OpenTK;

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
            Arguments = "<chunks/screen/all>";
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
            if (arg == "screen" || arg == "all")
            {
                success = true;
                TheClient.Window.ClientSize = new Size(TheClient.CVars.r_width.ValueI, TheClient.CVars.r_height.ValueI);
                TheClient.Window.WindowState = TheClient.CVars.r_fullscreen.ValueB ? WindowState.Fullscreen : WindowState.Normal;
                // TODO: Update all settings everywhere via (event/clientmethod/...?)
            }
            if (!success)
            {
                entry.Bad("Invalid argument.");
            }
            else
            {
                entry.Good("Successfully reloaded specified values.");
            }
        }
    }
}
