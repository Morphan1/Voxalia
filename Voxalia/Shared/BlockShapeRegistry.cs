//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Linq;
using System.Collections.Generic;
using Voxalia.Shared.BlockShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using FreneticScript;
using Voxalia.Shared.ModelManagement;

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

        static bool inited = false;

        public static void Init()
        {
            if (inited)
            {
                return;
            }
            inited = true;
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
            // Final setup
            int[] DB_TID = MaterialHelpers.ALL_MATS[(int)Material.DEBUG].TID;
            int lim = 0;
            for (int i = 0; i < DB_TID.Length; i++)
            {
                if (DB_TID[i] > lim)
                {
                    lim = DB_TID[i];
                }
            }
            int[] rlok = new int[lim + 1];
            for (int i = 0; i < DB_TID.Length; i++)
            {
                rlok[DB_TID[i]] = i;
            }
            for (int i = 0; i < 256; i++)
            {
                if (i > 0 && BSD[i] is BSD0)
                {
                    continue;
                }
                BSD[i].Preparse(rlok);
            }
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

    public class BlockShapeSubDetails
    {
        public List<Vector3>[] Verts = new List<Vector3>[64];
        public List<Vector3>[] Norms = new List<Vector3>[64];
        public Vector3[][] TCrds = new Vector3[64][];
    }

    /// <summary>
    /// Represents the details of a single block shape option.
    /// </summary>
    public abstract class BlockShapeDetails
    {
        public const double SHRINK_CONSTANT = 0.9;

        public double LightDamage = 1.0;

        public abstract List<Vector3> GetVertices(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM);

        public abstract List<Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM);

        public abstract List<Vector3> GetTCoords(Vector3 blockPos, Material mat, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM);

        public abstract bool OccupiesXP();

        public abstract bool OccupiesYP();

        public abstract bool OccupiesXM();

        public abstract bool OccupiesYM();

        public abstract bool OccupiesTOP();

        public abstract bool OccupiesBOTTOM();

        public BlockShapeSubDetails BSSD = new BlockShapeSubDetails();

        public BlockDamage DamageMode = BlockDamage.NONE;

        public void Preparse(int[] rlok)
        {
            DB_RLOK = rlok;
            for (int i = 0; i < 64; i++)
            {
                BSSD.Verts[i] = GetVertices(Vector3.Zero, (i & 1) == 1, (i & 2) == 2, (i & 4) == 4, (i & 8) == 8, (i & 16) == 16, (i & 32) == 32);
                BSSD.Norms[i] = GetNormals(Vector3.Zero, (i & 1) == 1, (i & 2) == 2, (i & 4) == 4, (i & 8) == 8, (i & 16) == 16, (i & 32) == 32);
                BSSD.TCrds[i] = GetTCoords(Vector3.Zero, Material.DEBUG, (i & 1) == 1, (i & 2) == 2, (i & 4) == 4, (i & 8) == 8, (i & 16) == 16, (i & 32) == 32).ToArray();
            }
            Damaged = new BlockShapeDetails[4];
            BlockShapeDetails prev = this;
            Damaged[0] = this;
            for (int i = 1; i < Damaged.Length; i++)
            {
                Damaged[i] = (BlockShapeDetails)prev.MemberwiseClone();
                Damaged[i].DamageMode = (BlockDamage)i;
                Damaged[i].Damage();
                prev = Damaged[i];
            }
        }

        private void Damage()
        {
            if (!CanSubdiv)
            {
                return;
            }
            if ((int)DamageMode > 1)
            {
                return; // Placeholder until simplify is added.
            }
            Subdivide();
        }

        public bool CanSubdiv = true;

        private void Subdivide()
        {
            // TODO: Save TCs and work with them properly.
            Shape p = new Shape();
            Dictionary<Location, Point> ps = new Dictionary<Location, Point>();
            for (int i = 0; i < BSSD.Verts[0].Count; i++)
            {
                Location t = new Location(BSSD.Verts[0][i]);
                if (!ps.ContainsKey(t))
                {
                    ps.Add(t, new Point(BSSD.Verts[0][i]));
                }
            }
            for (int i = 0; i < BSSD.Verts[0].Count; i += 3)
            {
                Point a = ps[new Location(BSSD.Verts[0][i])];
                Point b = ps[new Location(BSSD.Verts[0][i + 1])];
                Point c = ps[new Location(BSSD.Verts[0][i + 2])];
                if (i + 3 < BSSD.Verts[0].Count)
                {
                    Point a2 = ps[new Location(BSSD.Verts[0][i + 3])];
                    Point b2 = ps[new Location(BSSD.Verts[0][i + 4])];
                    Point c2 = ps[new Location(BSSD.Verts[0][i + 5])];
                    bool ac = a2 == a || a2 == b || a2 == c;
                    bool bc = b2 == a || b2 == b || b2 == c;
                    bool cc = c2 == a || c2 == b || c2 == c;
                    if (this is BSD0)
                    {
                        SysConsole.Output(OutputType.DEBUG, ac + ", " + bc + ", " + cc);
                    }
                    if (ac && bc && cc)
                    {
                        SysConsole.Output(OutputType.WARNING, this + " has weird setup: " + a + ", " + b + ", " + c);
                        p.AddFace(SubdivisionUtilities.CreateFaceF(p.AllEdges, a, b, c));
                    }
                    else if (ac && cc)
                    {
                        p.AddFace(SubdivisionUtilities.CreateFaceF(p.AllEdges, a, b, b2, c));
                        i += 3;
                    }
                    else if (ac && bc)
                    {
                        p.AddFace(SubdivisionUtilities.CreateFaceF(p.AllEdges, a, b, c, c2));
                        i += 3;
                    }
                    else if (bc && cc)
                    {
                        p.AddFace(SubdivisionUtilities.CreateFaceF(p.AllEdges, a, b, c, a2));
                        i += 3;
                    }
                    else
                    {
                        p.AddFace(SubdivisionUtilities.CreateFaceF(p.AllEdges, a, b, c));
                    }
                }
                else
                {
                    p.AddFace(SubdivisionUtilities.CreateFaceF(p.AllEdges, a, b, c));
                }
            }
            CatmullClarkSubdivider cmcs = new CatmullClarkSubdivider();
            Shape res = cmcs.Subdivide(p);
            List<Vector3> vecs = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector3> Tcs = new List<Vector3>();
            foreach (Face face in res.Faces)
            {
                for (int i = 0; i < 3; i++)
                {
                    vecs.Add(face.AllPoints[i].Position);
                    norms.Add(face.Normal);
                    Tcs.Add(new Vector3(0, 0, BSSD.TCrds[0][0].Z));
                }
            }
            BSSD = new BlockShapeSubDetails();
            Vector3[] tcrds = Tcs.ToArray();
            for (int i = 0; i < BSSD.Verts.Length; i++)
            {
                BSSD.Verts[i] = vecs;
                BSSD.Norms[i] = norms;
                BSSD.TCrds[i] = tcrds;
            }
        }

        public BlockShapeDetails[] Damaged;

        private int[] DB_RLOK;

        public Vector3[] GetTCoordsQuick(int index, Material mat)
        {
            // NOTE: This method is called very often by the client. Any optimization here will be very useful!
            Vector3[] set = BSSD.TCrds[index];
            int len = set.Length;
            Vector3[] vecs = new Vector3[len];
            Vector3 temp;
            int[] helper = MaterialHelpers.ALL_MATS[(int)mat].TID;
            for (int i = 0; i < len; i++)
            {
                temp = set[i];
                temp.Z = helper[DB_RLOK[(int)temp.Z]];
                vecs[i] = temp;
            }
            return vecs;
        }

        public bool BackTextureAllowed = true;
        
        public EntityShape BlockShapeCache;

        public EntityShape ShrunkBlockShapeCache;

        public Location OffsetCache;

        public Location ShrunkOffsetCache;

        public EntityCollidable Coll = null;

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

        public virtual EntityShape GetShape(BlockDamage damage, out Location offset, bool shrink)
        {
            if ((shrink ? ShrunkBlockShapeCache : BlockShapeCache) != null)
            {
                offset = (shrink ? ShrunkOffsetCache : OffsetCache);
                return (shrink ? ShrunkBlockShapeCache : BlockShapeCache);
            }
            List<Vector3> vecs = GetVertices(new Vector3(0, 0, 0), false, false, false, false, false, false);
            Vector3 offs;
            if (vecs.Count == 0)
            {
                throw new Exception("No vertices for shape " + this);
            }
            if (shrink)
            {
                for (int i = 0; i < vecs.Count; i++)
                {
                    vecs[i] = (vecs[i] - new Vector3(0.5f, 0.5f, 0.5f)) * SHRINK_CONSTANT + new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
            ConvexHullShape shape = new ConvexHullShape(vecs, out offs) { CollisionMargin = 0 };
            offset = new Location(offs);
            if (shrink)
            {
                ShrunkBlockShapeCache = shape;
                ShrunkOffsetCache = offset;
            }
            else
            {
                BlockShapeCache = shape;
                OffsetCache = offset;
            }
            return shape;
        }
    }
}
