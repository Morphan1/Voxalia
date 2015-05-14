using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A quick command to play a sound effect.
    /// </summary>
    class PlayCommand : AbstractCommand
    {
        public Client TheClient;

        public PlayCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "play";
            Description = "Plays a sound effect.";
            Arguments = "<soundname> [pitch] [volume]"; // TODO: Parse a location input as well
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string sfx = entry.GetArgument(0);
            float pitch = 1f;
            float gain = 1f;
            if (entry.Arguments.Count > 1)
            {
                pitch = Utilities.StringToFloat(entry.GetArgument(1));
            }
            if (entry.Arguments.Count > 2)
            {
                gain = Utilities.StringToFloat(entry.GetArgument(2));
            }
            TheClient.Sounds.Play(TheClient.Sounds.GetSound(sfx), false, Location.NaN, pitch, gain);
        }
    }
}
