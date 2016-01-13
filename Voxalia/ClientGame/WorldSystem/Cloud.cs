using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.WorldSystem
{
    public class Cloud
    {
        public Cloud(Region tregion, Location pos)
        {
            TheRegion = tregion;
            Position = pos;
        }

        public long CID;

        public Region TheRegion;

        public Location Position = Location.Zero;

        public Location Velocity = Location.Zero;

        public List<Location> Points = new List<Location>();

        public List<float> Sizes = new List<float>();

        public List<float> EndSizes = new List<float>();
    }
}
