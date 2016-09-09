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

        public static OpenTK.Matrix4 Convert(BEPUutilities.Matrix mat)
        {
            return new OpenTK.Matrix4((float)mat.M11, (float)mat.M12, (float)mat.M13, (float)mat.M14, (float)mat.M21, (float)mat.M22, (float)mat.M23,
                (float)mat.M24, (float)mat.M31, (float)mat.M32, (float)mat.M33, (float)mat.M34, (float)mat.M41, (float)mat.M42, (float)mat.M43, (float)mat.M44);
        }
    }
}
