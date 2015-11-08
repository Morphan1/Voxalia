using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.PlayerCommandSystem.RegionCommands
{
    class BlockfloodPlayerCommand: AbstractPlayerCommand
    {
        public BlockfloodPlayerCommand()
        {
            Name = "blockflood";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 2)
            {
                entry.Player.Network.SendMessage("/blockflood <material> <max radius>");
                return;
            }
            Material chosenMat = MaterialHelpers.FromNameOrNumber(entry.InputArguments[0]);
            float maxRad = Utilities.StringToFloat(entry.InputArguments[1]);
            if (maxRad > 20) // TODO: Config!
            {
                entry.Player.Network.SendMessage("Maximum radius is 20!");
                return;
            }
            Location start = entry.Player.GetPosition().GetBlockLocation() + new Location(0, 0, 1);
            FloodFrom(entry.Player.TheRegion, start, start, chosenMat, maxRad);
        }

        Location[] FloodDirs = new Location[] { Location.UnitX, Location.UnitY, -Location.UnitX, -Location.UnitY, -Location.UnitZ };

        void FloodFrom(Region tregion, Location start, Location c, Material mat, float maxRad)
        {
            if ((c - start).LengthSquared() > maxRad * maxRad)
            {
                return;
            }
            if (tregion.GetBlockMaterial(c) != Material.AIR)
            {
                return;
            }
            tregion.SetBlockMaterial(c, mat);
            foreach (Location dir in FloodDirs)
            {
                FloodFrom(tregion, start, c + dir, mat, maxRad);
            }
        }
    }
}
