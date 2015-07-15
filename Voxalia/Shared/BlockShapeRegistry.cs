using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared.BlockShapes;
using BEPUutilities;

namespace Voxalia.Shared
{
    public class BlockShapeRegistry
    {
        public static BlockShapeDetails[] BSD = new BlockShapeDetails[256];

        static BlockShapeRegistry()
        {
            BSD[0] = new BSD0();
        }
    }

    public abstract class BlockShapeDetails
    {
        public abstract List<Vector3> GetVertices(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM);

        public abstract List<Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM);

        public abstract List<Vector3> GetTCoords(Vector3 blockPos, Material mat, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM);

        public abstract bool OccupiesXP();

        public abstract bool OccupiesYP();

        public abstract bool OccupiesXM();

        public abstract bool OccupiesYM();

        public abstract bool OccupiesTOP();

        public abstract bool OccupiesBOTTOM();
    }
}
