using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    public class StancePlayerCommand: AbstractPlayerCommand
    {
        public StancePlayerCommand()
        {
            Name = "stance";
            Silent = true;
        }
        
        // TODO: Clientside?
        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.Network.SendMessage("^r^1/stance <stance>"); // TODO: ShowUsage
                return;
            }
            string stance = entry.InputArguments[0].ToLower();
            // TOOD: Implement!
            if (stance == "stand")
            {
                entry.Player.DesiredStance = Stance.Standing;
            }
            else if (stance == "crouch")
            {
                entry.Player.DesiredStance = Stance.Crouching;
            }
            else
            {
                entry.Player.Network.SendMessage("^r^1Unknown stance input."); // TODO: Languaging
            }
        }
    }
}
