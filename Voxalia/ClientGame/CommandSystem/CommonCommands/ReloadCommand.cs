using FreneticScript.CommandSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using FreneticScript;

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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            string arg = entry.GetArgument(queue, 0).ToLowerFast();
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
                TheClient.ShadersCheck();
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
                entry.Bad(queue, "Invalid argument.");
            }
            else
            {
                entry.Good(queue, "Successfully reloaded specified values.");
            }
        }
    }
}
