using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.PlayerCommandSystem.RegionCommands
{
    class BlockshapePlayerCommand: AbstractPlayerCommand
    {
        public BlockshapePlayerCommand()
        {
            Name = "blockshape";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "/blockshape <data> [color]"); // TODO: Color as separate command!
                return;
            }
            byte dat = (byte)Utilities.StringToInt(entry.InputArguments[0]);
            byte col = 0;
            if (entry.InputArguments.Count > 1)
            {
                col = (byte)Utilities.StringToInt(entry.InputArguments[1]);
            }
            Location eye = entry.Player.GetEyePosition();
            CollisionResult cr = entry.Player.TheRegion.Collision.RayTrace(eye, eye + entry.Player.ForwardVector() * 5, entry.Player.IgnoreThis);
            if (cr.Hit && cr.HitEnt == null)
            {
                Location block = cr.Position - cr.Normal * 0.01;
                Material mat = entry.Player.TheRegion.GetBlockMaterial(block);
                if (mat != Material.AIR)
                {
                    entry.Player.TheRegion.SetBlockMaterial(block, mat, dat, col);
                    entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "Set.");
                    return;
                }
            }
            entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE,"Failed to set: couldn't hit a block!");
        }
    }
}
