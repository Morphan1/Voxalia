using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Voxalia.Shared;

namespace Voxalia.ClientGame.OtherSystems
{
    public class ClientUtilities
    {
        public static Location Convert(Vector3 vec)
        {
            return new Location(vec.X, vec.Y, vec.Z);
        }

        public static Vector3 Convert(Location loc)
        {
            return new Vector3((float)loc.X, (float)loc.Y, (float)loc.Z);
        }
    }
}
