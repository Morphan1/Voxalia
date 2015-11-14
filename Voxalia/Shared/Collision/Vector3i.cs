using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.Shared.Collision
{
    public struct Vector3i : IEquatable<Vector3i>
    {
        public int X;
        public int Y;
        public int Z;

        public override int GetHashCode()
        {
            return X + Y + Z;
        }
        public bool Equals(Vector3i other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }
    }
}
