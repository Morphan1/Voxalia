using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUutilities;

namespace Voxalia.Shared.BlockShapes
{
    public class BSD0: BlockShapeDetails
    {
        public override List<Vector3> GetVertices(Vector3 pos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> vecs = new List<Vector3>();
            if (!TOP)
            {
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
            }
            if (!BOTTOM)
            {
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
            }
            if (!XP)
            {
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
            }
            if (!XM)
            {
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
            }
            if (!YP)
            {
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
            }
            if (!YM)
            {
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z));
                vecs.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                vecs.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
            }
            return vecs;
        }

        public override List<BEPUutilities.Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> vecs = new List<Vector3>();
            return vecs;
        }

        public override bool OccupiesXP()
        {
            return true;
        }

        public override bool OccupiesYP()
        {
            return true;
        }

        public override bool OccupiesXM()
        {
            return true;
        }

        public override bool OccupiesYM()
        {
            return true;
        }

        public override bool OccupiesTOP()
        {
            return true;
        }

        public override bool OccupiesBOTTOM()
        {
            return true;
        }
    }
}
