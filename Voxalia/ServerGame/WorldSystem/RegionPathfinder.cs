using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using FreneticScript;
using Priority_Queue;

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        // Thanks to fullwall for the reference sources this was built off
        public List<Location> FindPath(Location startloc, Location endloc, double maxRadius, double goaldist)
        {
            startloc = startloc.GetBlockLocation() + new Location(0.5, 0.5, 1.0);
            endloc = endloc.GetBlockLocation() + new Location(0.5, 0.5, 1.0);
            double mrsq = maxRadius * maxRadius;
            double gosq = goaldist * goaldist;
            PathFindNode start = new PathFindNode() { Internal = startloc, F = 0, G = 0 };
            PathFindNode end = new PathFindNode() { Internal = endloc, F = 0, G = 0 };
            SimplePriorityQueue<PathFindNode> open = new SimplePriorityQueue<PathFindNode>();
            HashSet<Location> closed = new HashSet<Location>();
            open.Enqueue(start, start.F);
            while (open.Count > 0)
            {
                PathFindNode next = open.Dequeue();
                if ((end.Internal - next.Internal).LengthSquared() < gosq)
                {
                    return Reconstruct(next);
                }
                closed.Add(next.Internal);
                foreach (Location neighbor in PathFindNode.Neighbors)
                {
                    Location neighb = next.Internal + neighbor;
                    if (closed.Contains(neighb))
                    {
                        continue;
                    }
                    if ((neighb - startloc).LengthSquared() > mrsq)
                    {
                        continue;
                    }
                    if (GetBlockMaterial(neighb).GetSolidity() != MaterialSolidity.NONSOLID) // TODO: Better solidity check
                    {
                        continue;
                    }
                    // TODO: Better in-air check? Perhaps based on node.parent!
                    if (GetBlockMaterial(neighb + new Location(0, 0, -1)).GetSolidity() == MaterialSolidity.NONSOLID
                        && GetBlockMaterial(neighb + new Location(0, 0, -2)).GetSolidity() == MaterialSolidity.NONSOLID)
                    {
                        continue;
                    }
                    PathFindNode node = new PathFindNode() { Internal = neighb };
                    node.G = next.G + next.Distance(node);
                    node.F = node.G + node.Distance(end);
                    node.Parent = next;
                    if (open.Contains(node))
                    {
                        continue;
                    }
                    open.Enqueue(node, node.F);
                }
            }
            return null;
        }

        List<Location> Reconstruct(PathFindNode node)
        {
            List<Location> locs = new List<Location>();
            while (node != null)
            {
                locs.Add(node.Internal);
                node = node.Parent;
            }
            locs.Reverse();
            return locs;
        }
    }

    public class PathFindNode: FastPriorityQueueNode, IComparable<PathFindNode>, IEquatable<PathFindNode>, IComparer<PathFindNode>, IEqualityComparer<PathFindNode>
    {
        public Location Internal;
        
        public double F;

        public double G;

        public PathFindNode Parent;

        public static Location[] Neighbors = new Location[] { Location.UnitX, Location.UnitY, Location.UnitZ, -Location.UnitX, -Location.UnitY, -Location.UnitZ };

        public double Distance(PathFindNode other)
        {
            return (Internal - other.Internal).Length();
        }

        public int CompareTo(PathFindNode other)
        {
            if (other.F > F)
            {
                return 1;
            }
            else if (F > other.F)
            {
                return -1;
            }
            return 0;
        }

        public bool Equals(PathFindNode other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Internal == this.Internal;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PathFindNode))
            {
                return false;
            }
            return Equals((PathFindNode)obj);
        }

        public static bool operator ==(PathFindNode self, PathFindNode other)
        {
            if (ReferenceEquals(self, null) && ReferenceEquals(other, null))
            {
                return true;
            }
            if (ReferenceEquals(self, null) || ReferenceEquals(other, null))
            {
                return false;
            }
            return self.Equals(other);
        }

        public static bool operator !=(PathFindNode self, PathFindNode other)
        {
            if (ReferenceEquals(self, null) && ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(self, null) || ReferenceEquals(other, null))
            {
                return true;
            }
            return !self.Equals(other);
        }

        public override int GetHashCode()
        {
            return Internal.GetHashCode();
        }

        public int Compare(PathFindNode x, PathFindNode y)
        {
            return x.CompareTo(y);
        }

        public bool Equals(PathFindNode x, PathFindNode y)
        {
            return x == y;
        }

        public int GetHashCode(PathFindNode obj)
        {
            return obj.GetHashCode();
        }
    }
}
