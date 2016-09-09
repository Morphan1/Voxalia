using System;
using System.Linq;
using System.Collections.Generic;
using Voxalia.Shared.BlockShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using FreneticScript;

namespace Voxalia.Shared
{
    /// <summary>
    /// Handles the block 'shapes' engine, the engine that powers all the potential 3D shapes a block can be in.
    /// </summary>
    public class BlockShapeRegistry
    {
        /// <summary>
        /// The internal array of block shape details.
        /// </summary>
        public static BlockShapeDetails[] BSD = new BlockShapeDetails[256];

        /// <summary>
        /// All names of all BSDs.
        /// </summary>
        public static Dictionary<string, int> BSD_Names = new Dictionary<string, int>();

        static BlockShapeRegistry()
        {
            for (int i = 0; i < 256; i++)
            {
                BSD[i] = new BSD0();
            }
            Register(0, new BSD0(), "default", "block", "standard", "cube", "plain");
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
            BSD[53] = new BSD53_54(0.25f, 0.75f, 0.5f);
            BSD[54] = new BSD53_54(0f, 1f, 1f);
            BSD[55] = new BSD55();
            BSD[56] = new BSD56();
            BSD[57] = new BSD57();
            BSD[58] = new BSD58();
            // ...
            BSD[64] = new BSD64_68(MaterialSide.BOTTOM, MaterialSide.XP, MaterialSide.XM, MaterialSide.YP, MaterialSide.YM, MaterialSide.TOP);
            BSD[65] = new BSD64_68(MaterialSide.XP, MaterialSide.XM, MaterialSide.YP, MaterialSide.YM, MaterialSide.TOP, MaterialSide.BOTTOM);
            BSD[66] = new BSD64_68(MaterialSide.XM, MaterialSide.YP, MaterialSide.YM, MaterialSide.TOP, MaterialSide.BOTTOM, MaterialSide.XP);
            BSD[67] = new BSD64_68(MaterialSide.YP, MaterialSide.YM, MaterialSide.TOP, MaterialSide.BOTTOM, MaterialSide.XP, MaterialSide.XM);
            BSD[68] = new BSD64_68(MaterialSide.YM, MaterialSide.TOP, MaterialSide.BOTTOM, MaterialSide.XP, MaterialSide.XM, MaterialSide.YP);
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

        public static int GetBSDFor(string name)
        {
            byte ret;
            if (byte.TryParse(name, out ret))
            {
                return ret;
            }
            int iret;
            if (BSD_Names.TryGetValue(name.ToLowerFast(), out iret))
            {
                return iret;
            }
            return 0;
        }

        static void Register(int ID, BlockShapeDetails bsd, params string[] names)
        {
            BSD[ID] = bsd;
            foreach (string str in names)
            {
                BSD_Names.Add(str, ID);
            }
        }
    }

    /// <summary>
    /// Represents the details of a single block shape option.
    /// </summary>
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

        public bool BackTextureAllowed = true;

        public virtual List<Vector4> GetLights(Vector3 blockPos, BlockInternal cblock, BlockInternal XP, BlockInternal XM, BlockInternal YP, BlockInternal YM, BlockInternal TOP, BlockInternal BOTTOM,
            bool bxp, bool bxm, bool byp, bool bym, bool btop, bool bbottom)
        {
            // TODO: Remove default implementation?
            List<Vector4> lits = new List<Vector4>(6 * 3 * 2);
            int c = GetVertices(blockPos, bxp, bxm, byp, bym, btop, bbottom).Count;
            for (int i = 0; i < c; i++)
            {
                double f = (double)cblock.BlockLocalData / 255f;
                lits.Add(new Vector4(f, f, f, 1f));
            }
            return lits;
        }

        public EntityShape BlockShapeCache;

        public Location OffsetCache;

        public virtual KeyValuePair<List<Vector4>, List<Vector4>> GetStretchData(Vector3 blockpos, List<Vector3> vertices, BlockInternal XP, BlockInternal XM,
            BlockInternal YP, BlockInternal YM, BlockInternal ZP, BlockInternal ZM, bool bxp, bool bxm, bool byp, bool bym, bool bzp, bool bzm)
        {
            List<Vector4> stretchvals = new List<Vector4>();
            List<Vector4> stretchweis = new List<Vector4>();
            for (int i = 0; i < vertices.Count; i++)
            {
                stretchvals.Add(new Vector4(0, 0, 0, 0));
                stretchweis.Add(new Vector4(0, 0, 0, 0));
            }
            return new KeyValuePair<List<Vector4>, List<Vector4>>(stretchvals, stretchweis);
        }

        public virtual EntityShape GetShape(BlockDamage damage, out Location offset)
        {
            if (BlockShapeCache != null)
            {
                offset = OffsetCache;
                return BlockShapeCache;
            }
            List<Vector3> vecs = GetVertices(new Vector3(0, 0, 0), false, false, false, false, false, false);
            Vector3 offs;
            if (vecs.Count == 0)
            {
                throw new Exception("No vertices for shape " + this);
            }
            ConvexHullShape shape = new ConvexHullShape(vecs, out offs) { CollisionMargin = 0 };
            offset = new Location(offs);
            OffsetCache = offset;
            BlockShapeCache = shape;
            return shape;
        }
    }
}
