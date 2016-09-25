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

        public static Location ConvertD(Vector3d vec)
        {
            return new Location(vec.X, vec.Y, vec.Z);
        }

        public static Vector3 Convert(Location loc)
        {
            return new Vector3((float)loc.X, (float)loc.Y, (float)loc.Z);
        }

        public static Vector3d ConvertD(Location loc)
        {
            return new Vector3d(loc.X, loc.Y, loc.Z);
        }

        public static OpenTK.Matrix4 Convert(BEPUutilities.Matrix mat)
        {
            return new OpenTK.Matrix4((float)mat.M11, (float)mat.M12, (float)mat.M13, (float)mat.M14, (float)mat.M21, (float)mat.M22, (float)mat.M23,
                (float)mat.M24, (float)mat.M31, (float)mat.M32, (float)mat.M33, (float)mat.M34, (float)mat.M41, (float)mat.M42, (float)mat.M43, (float)mat.M44);
        }

        public static OpenTK.Matrix4d ConvertD(BEPUutilities.Matrix mat)
        {
            return new OpenTK.Matrix4d(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        public static OpenTK.Matrix4d ConvertToD(OpenTK.Matrix4 mat)
        {
            return new OpenTK.Matrix4d(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }
    }
}
