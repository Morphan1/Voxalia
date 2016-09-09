using FreneticScript.CommandSystem;
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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            string sfx = entry.GetArgument(queue, 0);
            float pitch = 1f;
            float gain = 1f;
            Location loc = Location.NaN;
            if (entry.Arguments.Count > 1)
            {
                pitch = (float)Utilities.StringToFloat(entry.GetArgument(queue, 1));
            }
            if (entry.Arguments.Count > 2)
            {
                gain = (float)Utilities.StringToFloat(entry.GetArgument(queue, 2));
            }
            if (entry.Arguments.Count > 3)
            {
                loc = Location.FromString(entry.GetArgument(queue, 3));
            }
            float seek = 0;
            if (entry.Arguments.Count > 4)
            {
                seek = (float)(float)Utilities.StringToFloat(entry.GetArgument(queue, 4));
            }
            TheClient.Sounds.Play(TheClient.Sounds.GetSound(sfx), false, loc, pitch, gain, seek);
        }
    }
}
