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
            return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }
    }
}
