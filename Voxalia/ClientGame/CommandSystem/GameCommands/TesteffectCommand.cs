//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.GraphicsSystems.ParticleSystem;
using Voxalia.Shared;
using FreneticScript;

namespace Voxalia.ClientGame.CommandSystem.GameCommands
{
    public class TesteffectCommand: AbstractCommand
    {
        public Client TheClient;

        public TesteffectCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "testeffect";
            Description = "Quick-tests a particle effect, clientside.";
            Arguments = "effect";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            Location start = TheClient.Player.GetEyePosition();
            Location forward = TheClient.Player.ForwardVector();
            Location end = start + forward * 5;
            switch (entry.GetArgument(queue, 0).ToLowerFast())
            {
                case "cylinder":
                    TheClient.Particles.Engine.AddEffect(ParticleEffectType.CYLINDER, (o) => start, (o) => end, (o) => 0.01f, 5f, Location.One, Location.One, true, TheClient.Textures.GetTexture("common/smoke"));
                    break;
                case "line":
                    TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => start, (o) => end, (o) => 1f, 5f, Location.One, Location.One, true, TheClient.Textures.GetTexture("common/smoke"));
                    break;
                case "explosion_small":
                    TheClient.Particles.Explode(end, 2, 40);
                    break;
                case "explosion_large":
                    TheClient.Particles.Explode(end, 5);
                    break;
                case "path_mark":
                    TheClient.Particles.PathMark(end, () => TheClient.Player.GetPosition());
                    break;
                default:
                    entry.Bad(queue, "Unknown effect name.");
                    return;
            }
            entry.Good(queue, "Created effect.");
        }
    }
}
