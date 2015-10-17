using System.Collections.Generic;
using BEPUutilities;

namespace Voxalia.Shared.BlockShapes
{
    public class BSD39: BlockShapeDetails
    {
        public override BEPUphysics.CollisionShapes.EntityShape GetShape(out Location offset)
        {
            offset = new Location(0.5);
            return new BEPUphysics.CollisionShapes.ConvexShapes.BoxShape(1, 1, 1);
        }

        public override List<Vector3> GetVertices(Vector3 pos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Vertices = new List<Vector3>();
            if (!TOP)
            {
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.15f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.15f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.85f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.85f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.85f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.85f, pos.Z + 1));
                Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z + 1));
            }
            if (!BOTTOM)
            {
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.15f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.15f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.85f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.85f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.85f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.85f, pos.Z));
                Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z));
            }
            // 0.9191f, -0.3939f, 0f
            Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z));
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z));
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z));
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z + 1));
            // 0.3939f, -0.9191f, 0f
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z));
            Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.15f, pos.Z));
            Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z));
            Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z + 1));
            // 
            /*
            Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.15f, pos.Z + 1));
            //
            Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.15f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, pos.Z + 1));
            //
            Vertices.Add(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.85f, pos.Z + 1));
            //
            Vertices.Add(new Vector3(pos.X + 0.15f, pos.Y + 0.85f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, pos.Z + 1));
            //
            Vertices.Add(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.85f, pos.Z + 1));
            //
            Vertices.Add(new Vector3(pos.X + 0.85f, pos.Y + 0.85f, pos.Z + 1));
            Vertices.Add(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, pos.Z + 1));
            */
            return Vertices;
        }
        
        public override List<Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Norms = new List<Vector3>();
            if (!TOP)
            {
                for (int i = 0; i < 24; i++)
                {
                    Norms.Add(new Vector3(0, 0, 1));
                }
            }
            if (!BOTTOM)
            {
                for (int i = 0; i < 24; i++)
                {
                    Norms.Add(new Vector3(0, 0, -1));
                }
            }
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(0.9191f, -0.3939f, 0f));
            }
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(-0.3939f, 0.9191f, 0f));
            }
            
            return Norms;
        }

        public override List<Vector3> GetTCoords(Vector3 blockPos, Material mat, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> TCoords = new List<Vector3>();
            if (!TOP)
            {
                int tID_TOP = mat.TextureID(MaterialSide.TOP);
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(1.0f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.85f, 0.15f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.85f, 0.15f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.0f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.0f, tID_TOP));
                TCoords.Add(new Vector3(0.15f, 0.15f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.15f, 0.15f, tID_TOP));
                TCoords.Add(new Vector3(0.0f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.0f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.15f, 0.85f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.15f, 0.85f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 1.0f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 1.0f, tID_TOP));
                TCoords.Add(new Vector3(0.85f, 0.85f, tID_TOP));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_TOP));
                TCoords.Add(new Vector3(0.85f, 0.85f, tID_TOP));
                TCoords.Add(new Vector3(1.0f, 0.5f, tID_TOP));
            }
            if (!BOTTOM)
            {
                int tID_BOTTOM = mat.TextureID(MaterialSide.BOTTOM);
                TCoords.Add(new Vector3(0.85f, 0.15f, tID_BOTTOM));
                TCoords.Add(new Vector3(1.0f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.0f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.85f, 0.15f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.15f, 0.15f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.0f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.0f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.15f, 0.15f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.15f, 0.85f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.0f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 1.0f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.15f, 0.85f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.85f, 0.85f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 1.0f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(1.0f, 0.5f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.85f, 0.85f, tID_BOTTOM));
                TCoords.Add(new Vector3(0.5f, 0.5f, tID_BOTTOM));
            }
            // 0.9191f, -0.3939f, 0f
            int tID_XP = mat.TextureID(MaterialSide.XP);
            TCoords.Add(new Vector3(1, 1, tID_XP));
            TCoords.Add(new Vector3(0.5f, 0, tID_XP));
            TCoords.Add(new Vector3(1, 0, tID_XP));
            TCoords.Add(new Vector3(1, 1, tID_XP));
            TCoords.Add(new Vector3(0.5f, 1, tID_XP));
            TCoords.Add(new Vector3(0.5f, 0, tID_XP));
            // 0.3939f, -0.9191f, 0f
            int tID_YP = mat.TextureID(MaterialSide.YP);
            TCoords.Add(new Vector3(1, 1, tID_YP));
            TCoords.Add(new Vector3(0.5f, 0, tID_YP));
            TCoords.Add(new Vector3(1, 0, tID_YP));
            TCoords.Add(new Vector3(1, 1, tID_YP));
            TCoords.Add(new Vector3(0.5f, 1, tID_YP));
            TCoords.Add(new Vector3(0.5f, 0, tID_YP));
            return TCoords;
        }

        public override bool OccupiesXP()
        {
            return false;
        }

        public override bool OccupiesYP()
        {
            return false;
        }

        public override bool OccupiesXM()
        {
            return false;
        }

        public override bool OccupiesYM()
        {
            return false;
        }

        public override bool OccupiesTOP()
        {
            return false;
        }

        public override bool OccupiesBOTTOM()
        {
            return false;
        }
    }
}
