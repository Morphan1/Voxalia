using System;
using Voxalia.Shared;
using FreneticScript;
using BEPUutilities;

namespace Voxalia.ClientGame.GraphicsSystems
{
    /// <summary>
    /// Represents a triangle in 3D space.
    /// </summary>
    public class Plane
    {
        /// <summary>
        /// The normal of the plane.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The first corner.
        /// </summary>
        public Vector3 vec1;

        /// <summary>
        /// The second corner.
        /// </summary>
        public Vector3 vec2;

        /// <summary>
        /// The third corner.
        /// </summary>
        public Vector3 vec3;

        /// <summary>
        /// The distance from the origin.
        /// </summary>
        public float D;

        public Plane(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            vec1 = v1;
            vec2 = v2;
            vec3 = v3;
            Normal = Vector3.Cross((v2 - v1), (v3 - v1));
            Normal.Normalize();
            D = -Vector3.Dot(Normal, vec1);
        }

        public Plane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 _normal)
        {
            vec1 = v1;
            vec2 = v2;
            vec3 = v3;
            Normal = _normal;
            D = -Vector3.Dot(Normal, vec1);
        }

        public Plane(Vector3 _normal, float _d)
        {
            float fact = 1f / _normal.Length();
            Normal = _normal * fact;
            D = _d * fact;
        }

        /// <summary>
        /// Finds where a line hits the plane, if anywhere.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <returns>A location of the hit, or NaN if none.</returns>
        public Vector3 IntersectLine(Vector3 start, Vector3 end)
        {
            Vector3 ba = end - start;
            float nDotA = Vector3.Dot(Normal, start);
            float nDotBA = Vector3.Dot(Normal, ba);
            float t = -(nDotA + D) / (nDotBA);
            if (t < 0) // || t > 1
            {
                return new Vector3(float.NaN, float.NaN, float.NaN);
            }
            return start + t * ba;
        }

        public Plane FlipNormal()
        {
            return new Plane(vec3, vec2, vec1, -Normal);
        }

        /// <summary>
        /// Returns the distance between a point and the plane.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The distance.</returns>
        public float Distance(Vector3 point)
        {
            return Vector3.Dot(Normal, point) + D;
        }

        /// <summary>
        /// Determines the signs of a box to the plane.
        /// If it returns 1, the box is above the plane.
        /// If it returns -1, the box is below the plane.
        /// If it returns 0, the box intersections with the plane.
        /// </summary>
        /// <param name="Mins">The mins of the box.</param>
        /// <param name="Maxs">The maxes of the box.</param>
        /// <returns>-1, 0, or 1.</returns>
        public int SignToPlane(Vector3 Mins, Vector3 Maxs)
        {
            Vector3[] locs = new Vector3[8];
            locs[0] = new Vector3(Mins.X, Mins.Y, Mins.Z);
            locs[1] = new Vector3(Mins.X, Mins.Y, Maxs.Z);
            locs[2] = new Vector3(Mins.X, Maxs.Y, Mins.Z);
            locs[3] = new Vector3(Mins.X, Maxs.Y, Maxs.Z);
            locs[4] = new Vector3(Maxs.X, Mins.Y, Mins.Z);
            locs[5] = new Vector3(Maxs.X, Mins.Y, Maxs.Z);
            locs[6] = new Vector3(Maxs.X, Maxs.Y, Mins.Z);
            locs[7] = new Vector3(Maxs.X, Maxs.Y, Maxs.Z);
            int psign = Math.Sign(Distance(locs[0]));
            for (int i = 1; i < locs.Length; i++)
            {
                if (Math.Sign(Distance(locs[i])) != psign)
                {
                    return 0;
                }
            }
            return psign;
        }
        
        public override string ToString()
        {
            return "[" + vec1.ToString() + "/" + vec2.ToString() + "/" + vec3.ToString() + "]";
        }

        /// <summary>
        /// Converts a string to a plane.
        /// </summary>
        /// <param name="input">The plane string.</param>
        /// <returns>A plane.</returns>
        public static Plane FromString(string input)
        {
            string[] data = input.Replace("[", "").Replace("]", "").Replace(" ", "").SplitFast('/');
            if (data.Length < 3)
            {
                return null;
            }
            return new Plane(Location.FromString(data[0]).ToBVector(), Location.FromString(data[1]).ToBVector(), Location.FromString(data[2]).ToBVector());
        }
    }
}
