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
                entry.Player.CBody.StanceManager.DesiredStance = Stance.Standing;
            }
            else if (stance == "crouch")
            {
                entry.Player.CBody.StanceManager.DesiredStance = Stance.Crouching;
            }
            else
            {
                entry.Player.Network.SendMessage("^r^1Unknown stance input."); // TODO: Languaging
            }
        }
    }
}
