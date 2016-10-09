//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using FreneticScript;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    public class ReloadGameCommand: AbstractCommand
    {
        public Client TheClient;

        public ReloadGameCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "reloadgame";
            Description = "Reloads all or part of the game.";
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
                success = true;
                TheClient.TheRegion.RenderingNow.Clear();
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
                TheClient.ShadersCheck();
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
                TheClient.Textures.InitTextureSystem(TheClient);
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
