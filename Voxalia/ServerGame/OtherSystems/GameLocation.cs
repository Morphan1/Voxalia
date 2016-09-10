using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.OtherSystems
{
    public class GameLocation
    {
        public GameLocation(Location coord, World w)
        {
            Coordinates = coord;
            World = w;
        }

        public Location Coordinates;

        public World World;
    }
}
