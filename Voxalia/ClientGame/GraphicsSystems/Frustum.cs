using System;
using Voxalia.Shared;
using BEPUutilities;

namespace Voxalia.ClientGame.GraphicsSystems
{
    /// <summary>
    /// Represents a 3D Frustum.
    /// Can be used to represent the area a camera can see.
    /// Can be used for high-speed culling of visible objects.
    /// </summary>
    public class Frustum
    {
        public Plane[] Planes = new Plane[6];

        public Frustum(OpenTK.Matrix4d matrix)
        {
            Planes[0] = new Plane(new Vector3(-matrix.M14 - matrix.M11, -matrix.M24 - matrix.M21, -matrix.M34 - matrix.M31), -matrix.M44 - matrix.M41);
            Planes[1] = new Plane(new Vector3(matrix.M11 - matrix.M14, matrix.M21 - matrix.M24, matrix.M31 - matrix.M34), matrix.M41 - matrix.M44);
            Planes[2] = new Plane(new Vector3(matrix.M12 - matrix.M14, matrix.M22 - matrix.M24, matrix.M32 - matrix.M34), matrix.M42 - matrix.M44);
            Planes[3] = new Plane(new Vector3(-matrix.M14 - matrix.M12, -matrix.M24 - matrix.M22, -matrix.M34 - matrix.M32), -matrix.M44 - matrix.M42);
            Planes[4] = new Plane(new Vector3(-matrix.M13, -matrix.M23, -matrix.M33), -matrix.M43);
            Planes[5] = new Plane(new Vector3(matrix.M13 - matrix.M14, matrix.M23 - matrix.M24, matrix.M33 - matrix.M34), matrix.M43 - matrix.M44);
        }

        /// <summary>
        /// Returns whether an AABB is contained by the Frustum.
        /// </summary>
        /// <param name="min">The lower coord of the AABB.</param>
        /// <param name="max">The higher coord of the AABB.</param>
        /// <returns>Whether it is contained.</returns>
        public bool ContainsBox(Vector3 min, Vector3 max)
        {
            if (min == max)
            {
                return Contains(min);
            }
            Vector3[] locs = new Vector3[] {
                min, max, new Vector3(min.X, min.Y, max.Z),
                new Vector3(min.X, max.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(max.X, max.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z)
            };
            for (int p = 0; p < 6; p++)
            {
                int inC = 8;
                for (int i = 0; i < 8; i++)
                {
                    if (Math.Sign(Planes[p].Distance(locs[i])) == 1)
                    {
                        inC--;
                    }
                }
                if (inC == 0)
                {
                    /*
                    // Backup
                    if (Contains(min)) { return true; }
                    else if (Contains(max)) { return true; }
                    else if (Contains(new Vector3(min.X, min.Y, max.Z))) { return true; }
                    else if (Contains(new Vector3(min.X, max.Y, max.Z))) { return true; }
                    else if (Contains(new Vector3(max.X, min.Y, max.Z))) { return true; }
                    else if (Contains(new Vector3(max.X, min.Y, min.Z))) { return true; }
                    else if (Contains(new Vector3(max.X, max.Y, min.Z))) { return true; }
                    else if (Contains(new Vector3(min.X, max.Y, min.Z))) { return true; }*/
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns whether the frustum contains a sphere.
        /// </summary>
        /// <param name="point">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <returns>Whether it intersects.</returns>
        public bool ContainsSphere(Vector3 point, float radius)
        {
            return ContainsBox(point - new Vector3(radius, radius, radius), point + new Vector3(radius, radius, radius));
        }

        /// <summary>
        /// Returns whether the Frustum contains a point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Whether it's contained.</returns>
        public bool Contains(Vector3 point)
        {
            if (TryPoint(point, Planes[5]) > 0) { return false; }
            if (TryPoint(point, Planes[4]) > 0) { return false; }
            if (TryPoint(point, Planes[3]) > 0) { return false; }
            if (TryPoint(point, Planes[2]) > 0) { return false; }
            if (TryPoint(point, Planes[1]) > 0) { return false; }
            if (TryPoint(point, Planes[0]) > 0) { return false; }
            return true;
        }

        float TryPoint(Vector3 point, Plane plane)
        {
            return (float)(point.X * plane.Normal.X + point.Y * plane.Normal.Y + point.Z * plane.Normal.Z + plane.D);
        }
    }
}
