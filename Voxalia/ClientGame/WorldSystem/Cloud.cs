//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
