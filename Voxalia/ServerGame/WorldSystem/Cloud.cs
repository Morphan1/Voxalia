using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem
{
    public class Cloud
    {
        public Cloud(Region tregion, Location pos)
        {
            TheRegion = tregion;
            Position = pos;
        }
        
        public long CID;

        public bool IsNew = true;

        public Region TheRegion;

        public Location Position = Location.Zero;

        public Location Velocity = Location.Zero;

        public List<Location> Points = new List<Location>();

        public List<double> Sizes = new List<double>();

        public List<double> EndSizes = new List<double>();
    }
}
