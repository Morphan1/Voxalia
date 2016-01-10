using FreneticScript.CommandSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.ClientMainSystem;
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
            Arguments = "<chunks/screen/shaders/audio/textures/all>"; // TODO: List input?
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string arg = entry.GetArgument(0).ToLowerInvariant();
            bool success = false;
            bool is_all = arg == "all";
            if (arg == "chunks" || is_all)
            {
                // TODO: Goto load screen?
                success = true;
                foreach (Chunk chunk in TheClient.TheRegion.LoadedChunks.Values)
                {
                    chunk.OwningRegion.UpdateChunk(chunk);
                }
            }
            if (arg == "screen" || is_all)
            {
                success = true;
                TheClient.UpdateWindow();
            }
            if (arg == "shaders" || is_all)
            {
                success = true;
                TheClient.Shaders.Clear();
            }
            if (arg == "audio" || is_all)
            {
                success = true;
                TheClient.Sounds.Init(TheClient, TheClient.CVars);
            }
            if (arg == "textures" || is_all)
            {
                success = true;
                TheClient.Textures.Empty();
                TheClient.Textures.InitTextureSystem();
                TheClient.TBlock.Generate(TheClient, TheClient.CVars, TheClient.Textures);
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
