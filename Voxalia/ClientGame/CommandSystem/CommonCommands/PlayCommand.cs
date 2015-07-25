using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
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
            Arguments = "<soundname> [pitch] [volume] [location] [seek time in seconds]";
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
            Location loc = Location.NaN;
            if (entry.Arguments.Count > 1)
            {
                pitch = Utilities.StringToFloat(entry.GetArgument(1));
            }
            if (entry.Arguments.Count > 2)
            {
                gain = Utilities.StringToFloat(entry.GetArgument(2));
            }
            if (entry.Arguments.Count > 3)
            {
                loc = Location.FromString(entry.GetArgument(3));
            }
            float seek = 0;
            if (entry.Arguments.Count > 4)
            {
                seek = Utilities.StringToFloat(entry.GetArgument(4));
            }
            TheClient.Sounds.Play(TheClient.Sounds.GetSound(sfx), false, loc, pitch, gain, seek);
        }
    }
}
