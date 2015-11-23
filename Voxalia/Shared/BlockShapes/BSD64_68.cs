using System.Collections.Generic;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace Voxalia.Shared.BlockShapes
{
    public class BSD64_68: BlockShapeDetails
    {
        public MaterialSide[] Mats;

        public BSD64_68(params MaterialSide[] mats)
        {
            Mats = mats;
            BlockShapeCache = new BoxShape(1f, 1f, 1f);
            OffsetCache = new Location(0.5);
        }

        public override List<Vector3> GetVertices(Vector3 pos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Vertices = new List<Vector3>();
            if (!TOP)
            {
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
            }
            if (!BOTTOM)
            {
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
            }
            if (!XP)
            {
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
            }
            if (!XM)
            {
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
            }
            if (!YP)
            {
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
            }
            if (!YM)
            {
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
            }
            return Vertices;
        }

        public override List<Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Norms = new List<Vector3>();
            if (!TOP)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(0, 0, 1));
                }
            }
            if (!BOTTOM)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(0, 0, -1));
                }
            }
            if (!XP)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(1, 0, 0));
                }
            }
            if (!XM)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(-1, 0, 0));
                }
            }
            if (!YP)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(0, 1, 0));
                }
            }
            if (!YM)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(0, -1, 0));
                }
            }
            return Norms;
        }

        public override List<Vector3> GetTCoords(Vector3 blockPos, Material mat, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> TCoords = new List<Vector3>();
            if (!TOP)
            {
                int tID_TOP = mat.TextureID(Mats[0]);
                TCoords.Add(new Vector3(0, 1, tID_TOP));
                TCoords.Add(new Vector3(1, 1, tID_TOP));
                TCoords.Add(new Vector3(0, 0, tID_TOP));
                TCoords.Add(new Vector3(1, 1, tID_TOP));
                TCoords.Add(new Vector3(1, 0, tID_TOP));
                TCoords.Add(new Vector3(0, 0, tID_TOP));
            }
            if (!BOTTOM)
            {
                int tID_BOTTOM = mat.TextureID(Mats[1]);
                TCoords.Add(new Vector3(0, 0, tID_BOTTOM));
                TCoords.Add(new Vector3(1, 1, tID_BOTTOM));
                TCoords.Add(new Vector3(0, 1, tID_BOTTOM));
                TCoords.Add(new Vector3(0, 0, tID_BOTTOM));
                TCoords.Add(new Vector3(1, 0, tID_BOTTOM));
                TCoords.Add(new Vector3(1, 1, tID_BOTTOM));
            }
            if (!XP)
            {
                int tID_XP = mat.TextureID(Mats[2]);
                TCoords.Add(new Vector3(1, 0, tID_XP));
                TCoords.Add(new Vector3(1, 1, tID_XP));
                TCoords.Add(new Vector3(0, 1, tID_XP));
                TCoords.Add(new Vector3(0, 0, tID_XP));
                TCoords.Add(new Vector3(1, 0, tID_XP));
                TCoords.Add(new Vector3(0, 1, tID_XP));
            }
            if (!XM)
            {
                int tID_XM = mat.TextureID(Mats[3]);
                TCoords.Add(new Vector3(1, 1, tID_XM));
                TCoords.Add(new Vector3(0, 1, tID_XM));
                TCoords.Add(new Vector3(0, 0, tID_XM));
                TCoords.Add(new Vector3(1, 1, tID_XM));
                TCoords.Add(new Vector3(0, 0, tID_XM));
                TCoords.Add(new Vector3(1, 0, tID_XM));
            }
            if (!YP)
            {
                int tID_YP = mat.TextureID(Mats[4]);
                TCoords.Add(new Vector3(1, 1, tID_YP));
                TCoords.Add(new Vector3(0, 1, tID_YP));
                TCoords.Add(new Vector3(0, 0, tID_YP));
                TCoords.Add(new Vector3(1, 1, tID_YP));
                TCoords.Add(new Vector3(0, 0, tID_YP));
                TCoords.Add(new Vector3(1, 0, tID_YP));
            }
            if (!YM)
            {
                int tID_YM = mat.TextureID(Mats[5]);
                TCoords.Add(new Vector3(1, 0, tID_YM));
                TCoords.Add(new Vector3(1, 1, tID_YM));
                TCoords.Add(new Vector3(0, 1, tID_YM));
                TCoords.Add(new Vector3(0, 0, tID_YM));
                TCoords.Add(new Vector3(1, 0, tID_YM));
                TCoords.Add(new Vector3(0, 1, tID_YM));
            }
            return TCoords;
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
