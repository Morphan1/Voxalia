using System.Collections.Generic;
using Voxalia.Shared.BlockShapes;
using BEPUutilities;

namespace Voxalia.Shared
{
    public class BlockShapeRegistry
    {
        public static BlockShapeDetails[] BSD = new BlockShapeDetails[256];

        static BlockShapeRegistry()
        {
            for (int i = 0; i < 256; i++)
            {
                BSD[i] = new BSD0();
            }
            BSD[0] = new BSD0();
            BSD[1] = new BSD01_5(0.84f);
            BSD[2] = new BSD01_5(0.68f);
            BSD[3] = new BSD01_5(0.50f);
            BSD[4] = new BSD01_5(0.34f);
            BSD[5] = new BSD01_5(0.13f);
            BSD[6] = new BSD06_10(0.84f);
            BSD[7] = new BSD06_10(0.68f);
            BSD[8] = new BSD06_10(0.50f);
            BSD[9] = new BSD06_10(0.34f);
            BSD[10] = new BSD06_10(0.13f);
            BSD[11] = new BSD11_15(0.84f);
            BSD[12] = new BSD11_15(0.68f);
            BSD[13] = new BSD11_15(0.50f);
            BSD[14] = new BSD11_15(0.34f);
            BSD[15] = new BSD11_15(0.13f);
            BSD[16] = new BSD16_20(0.84f);
            BSD[17] = new BSD16_20(0.68f);
            BSD[18] = new BSD16_20(0.50f);
            BSD[19] = new BSD16_20(0.34f);
            BSD[20] = new BSD16_20(0.13f);
            BSD[21] = new BSD21_25(0.84f);
            BSD[22] = new BSD21_25(0.68f);
            BSD[23] = new BSD21_25(0.50f);
            BSD[24] = new BSD21_25(0.34f);
            BSD[25] = new BSD21_25(0.13f);
            BSD[26] = new BSD26_30(0.84f);
            BSD[27] = new BSD26_30(0.68f);
            BSD[28] = new BSD26_30(0.50f);
            BSD[29] = new BSD26_30(0.34f);
            BSD[30] = new BSD26_30(0.13f);
            BSD[31] = new BSD31();
            BSD[32] = new BSD32();
            BSD[33] = new BSD33();
            BSD[34] = new BSD34();
            // ...
            BSD[39] = new BSD39a76(1f);
            // ...
            BSD[52] = new BSD52a127(0.25f, 0.75f, 0.5f);
            // ...
            BSD[72] = new BSD72();
            BSD[73] = new BSD73();
            BSD[74] = new BSD74();
            BSD[75] = new BSD75();
            BSD[76] = new BSD39a76(0.5f);
            // ...
            BSD[80] = new BSD80();
            BSD[81] = new BSD81();
            BSD[82] = new BSD82();
            BSD[83] = new BSD83();
            // ...
            BSD[127] = new BSD52a127(0f, 1f, 1f);
            // ...
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

        public virtual List<Vector4> GetLights(Vector3 blockPos, BlockInternal cblock, BlockInternal XP, BlockInternal XM, BlockInternal YP, BlockInternal YM, BlockInternal TOP, BlockInternal BOTTOM,
            bool bxp, bool bxm, bool byp, bool bym, bool btop, bool bbottom)
        {
            // TODO: Remove default implementation
            List<Vector4> lits = new List<Vector4>(6 * 3 * 2);
            int c = GetVertices(blockPos, bxp, bxm, byp, bym, btop, bbottom).Count;
            for (int i = 0; i < c; i++)
            {
                float f = (float)cblock.BlockLocalData / 255f;
                lits.Add(new Vector4(f, f, f, 1f));
            }
            return lits;
        }

        public virtual BEPUphysics.CollisionShapes.EntityShape GetShape(out Location offset)
        {
            List<Vector3> vecs = GetVertices(new Vector3(0, 0, 0), false, false, false, false, false, false);
            int[] ints = new int[vecs.Count];
            for (int i = 0; i < vecs.Count; i++)
            {
                ints[i] = i;
            }
            Vector3 offs;
            BEPUphysics.CollisionShapes.MobileMeshShape Shape = new BEPUphysics.CollisionShapes.MobileMeshShape(vecs.ToArray(), ints, new AffineTransform(new Vector3(0.95f, 0.95f, 0.95f), // TODO: Fiddle!
                Quaternion.Identity, Vector3.Zero), BEPUphysics.CollisionShapes.MobileMeshSolidity.Solid, out offs);
            offset = new Location(offs);
            return Shape;
        }
    }
}
