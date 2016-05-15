using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities.ResourceManagement;
using BEPUphysics.Settings;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using BEPUutilities.DataStructures;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests;

namespace Voxalia.Shared.Collision
{
    public class MCCFCOContactManifold : ContactManifold
    {
        static LockingResourcePool<GeneralConvexPairTester> testerPool = new LockingResourcePool<GeneralConvexPairTester>();

        protected MobileChunkCollidable mobile;

        protected FullChunkObject mesh;

        public QuickDictionary<Vector3i, QuickList<GeneralConvexPairTester>> ActivePairs;
        private QuickDictionary<Vector3i, QuickList<GeneralConvexPairTester>> activePairsBackBuffer;
        protected RawValueList<ContactSupplementData> supplementData = new RawValueList<ContactSupplementData>(4);

        public override void Initialize(Collidable newCollidableA, Collidable newCollidableB)
        {
            mobile = newCollidableA as MobileChunkCollidable;
            mesh = newCollidableB as FullChunkObject;
            if (mobile == null || mesh == null)
            {
                mobile = newCollidableB as MobileChunkCollidable;
                mesh = newCollidableA as FullChunkObject;
                if (mobile == null || mesh == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize contact manifold.");
                }
            }
            ActivePairs = new QuickDictionary<Vector3i, QuickList<GeneralConvexPairTester>>(BufferPools<Vector3i>.Locking, BufferPools<QuickList<GeneralConvexPairTester>>.Locking, BufferPools<int>.Locking, 3);
            activePairsBackBuffer = new QuickDictionary<Vector3i, QuickList<GeneralConvexPairTester>>(BufferPools<Vector3i>.Locking, BufferPools<QuickList<GeneralConvexPairTester>>.Locking, BufferPools<int>.Locking, 3);
        }

        public MCCFCOContactManifold()
        {
            contacts = new RawList<Contact>(10);
            unusedContacts = new UnsafeResourcePool<Contact>(10);
            contactIndicesToRemove = new RawList<int>(10);
        }

        public RawList<Contact> ctcts
        {
            get
            {
                return contacts;
            }
        }

        private QuickList<GeneralConvexPairTester> GetPairs(ref Vector3i pos1, ref Vector3i position)
        {
            QuickList<GeneralConvexPairTester> pairs = new QuickList<GeneralConvexPairTester>();
            Vector3 offs;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        ConvexShape shape = mesh.ChunkShape.ShapeAt(position.X + x, position.Y + y, position.Z + z, out offs);
                        if (shape == null)
                        {
                            continue;
                        }
                        ReusableGenericCollidable<ConvexShape> boxCollidable = new ReusableGenericCollidable<ConvexShape>(shape);
                        RigidTransform rtb = new RigidTransform(new Vector3(
                            mesh.Position.X + position.X + x + offs.X,
                            mesh.Position.Y + position.Y + y + offs.Y,
                            mesh.Position.Z + position.Z + z + offs.Z));
                        boxCollidable.WorldTransform = rtb;
                        GeneralConvexPairTester pair = testerPool.Take();
                        ReusableGenericCollidable<ConvexShape> tempCollidable = new ReusableGenericCollidable<ConvexShape>(mobile.ChunkShape.ShapeAt(pos1.X, pos1.Y, pos1.Z, out offs));
                        Vector3 input = new Vector3(pos1.X + offs.X, pos1.Y + offs.Y, pos1.Z + offs.Z);
                        RigidTransform rt = mobile.WorldTransform;
                        Vector3 transfd = Quaternion.Transform(input, rt.Orientation);
                        RigidTransform outp = new RigidTransform(transfd + rt.Position, rt.Orientation);
                        tempCollidable.WorldTransform = outp;
                        pair.Initialize(tempCollidable, boxCollidable);
                    }
                }
            }
            return pairs;
        }
        
        private void ReturnPair(GeneralConvexPairTester pair)
        {
            pair.CleanUp();
            testerPool.GiveBack(pair);
        }

        public static bool IsNaNOrInfOrZero(ref Vector3 vec)
        {
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X)
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y)
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z) || (vec.X == 0 && vec.Y == 0 && vec.Z == 0);
        }

        public static bool IsNaNOrInf(ref Vector3 vec)
        {
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X)
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y)
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z);
        }
        
        public override void Update(float dt)
        {
            RigidTransform transform = new RigidTransform(mesh.Position);
            RigidTransform convexTransform = mobile.WorldTransform;
            ContactRefresher.ContactRefresh(contacts, supplementData, ref convexTransform, ref transform, contactIndicesToRemove);
            RemoveQueuedContacts();
            var overlaps = new QuickList<Vector3i>(BufferPools<Vector3i>.Thread);
            mesh.ChunkShape.GetOverlaps(mesh.Position, mobile.BoundingBox, ref overlaps);
            var candidatesToAdd = new QuickList<ContactData>(BufferPools<ContactData>.Thread, BufferPool<int>.GetPoolIndex(overlaps.Count));
            for (int i = 0; i < overlaps.Count; i++)
            {
                QuickList<GeneralConvexPairTester> manifolds;
                //if (!ActivePairs.TryGetValue(overlaps.Elements[i], out manifolds))
                //{
                Vector3i cur = overlaps.Elements[i];
                Vector3 curf = new Vector3(cur.X, cur.Y, cur.Z);
                Vector3 cf;
                RigidTransform.Transform(ref curf, ref transform, out cf);
                RigidTransform.TransformByInverse(ref cf, ref convexTransform, out cf);
                Vector3i holder = new Vector3i((int)cf.X, (int)cf.Y, (int)cf.Z);
                Vector3i size = mobile.ChunkShape.ChunkSize;
                if (holder.X >= size.X || holder.Y >= size.Y || holder.Z >= size.Z
                    || holder.X < 0 || holder.Y < 0 || holder.Z < 0)
                {
                    continue;
                }
                manifolds = GetPairs(ref holder, ref overlaps.Elements[i]);
                //}
                //else
                //{
                //    ActivePairs.FastRemove(overlaps.Elements[i]);
                //}
                activePairsBackBuffer.Add(overlaps.Elements[i], manifolds);
                for (int x = 0; x < manifolds.Count; x++)
                {
                    ContactData contactCandidate;
                    if (manifolds[x].GenerateContactCandidate(out contactCandidate))
                    {
                        candidatesToAdd.Add(ref contactCandidate);
                    }
                }
            }
            overlaps.Dispose();
            for (int i = ActivePairs.Count - 1; i >= 0; i--)
            {
                for (int x = 0; x < ActivePairs.Values[i].Count; x++)
                {
                    ReturnPair(ActivePairs.Values[i][x]);
                }
                //ActivePairs.FastRemove(ActivePairs.Keys[i]);
            }
            ActivePairs.FastClear();
            var temp = ActivePairs;
            ActivePairs = activePairsBackBuffer;
            activePairsBackBuffer = temp;
            if (contacts.Count + candidatesToAdd.Count > 4)
            {
                var reducedCandidates = new QuickList<ContactData>(BufferPools<ContactData>.Thread, 3);
                ContactReducer.ReduceContacts(contacts, ref candidatesToAdd, contactIndicesToRemove, ref reducedCandidates);
                RemoveQueuedContacts();
                for (int i = reducedCandidates.Count - 1; i >= 0; i--)
                {
                    Add(ref reducedCandidates.Elements[i]);
                    reducedCandidates.RemoveAt(i);
                }
                reducedCandidates.Dispose();
            }
            else if (candidatesToAdd.Count > 0)
            {
                for (int i = 0; i < candidatesToAdd.Count; i++)
                {
                    Add(ref candidatesToAdd.Elements[i]);
                }
            }
            candidatesToAdd.Dispose();
        }
        
        protected override void Add(ref ContactData contactCandidate)
        {
            ContactSupplementData supplement;
            supplement.BasePenetrationDepth = contactCandidate.PenetrationDepth;
            var convexTransform = mobile.WorldTransform;
            var gridTransform = new RigidTransform(mesh.Position);
            RigidTransform.TransformByInverse(ref contactCandidate.Position, ref convexTransform, out supplement.LocalOffsetA);
            RigidTransform.TransformByInverse(ref contactCandidate.Position, ref gridTransform, out supplement.LocalOffsetB);
            supplementData.Add(ref supplement);
            base.Add(ref contactCandidate);
        }

        protected override void Remove(int contactIndex)
        {
            supplementData.RemoveAt(contactIndex);
            base.Remove(contactIndex);
        }

        public override void CleanUp()
        {
            mobile = null;
            mesh = null;
            for (int i = ActivePairs.Count - 1; i >= 0; --i)
            {
                for (int x = 0; x < ActivePairs.Values[i].Count; x++)
                {
                    ReturnPair(ActivePairs.Values[i][x]);
                    ActivePairs.Values[i][x].CleanUp();
                }
            }
            ActivePairs.Clear();
            ActivePairs.Dispose();
            activePairsBackBuffer.Dispose();
            base.CleanUp();
        }
    }
}
