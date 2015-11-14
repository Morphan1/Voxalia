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
    public class FCOContactManifold : ContactManifold
    {
        private class ReusableBoxCollidable : ConvexCollidable<BoxShape>
        {
            public ReusableBoxCollidable()
                : base(new BoxShape(1, 1, 1))
            {
            }

            protected override void UpdateBoundingBoxInternal(float dt)
            {
                Shape.GetBoundingBox(ref worldTransform, out boundingBox);
            }
        }

        static LockingResourcePool<ReusableBoxCollidable> boxCollidablePool = new LockingResourcePool<ReusableBoxCollidable>();

        static LockingResourcePool<GeneralConvexPairTester> testerPool = new LockingResourcePool<GeneralConvexPairTester>();

        protected ConvexCollidable convex;

        protected FullChunkObject mesh;

        private QuickDictionary<Vector3i, GeneralConvexPairTester> activePairs;
        private QuickDictionary<Vector3i, GeneralConvexPairTester> activePairsBackBuffer;
        protected RawValueList<ContactSupplementData> supplementData = new RawValueList<ContactSupplementData>(4);

        public override void Initialize(Collidable newCollidableA, Collidable newCollidableB)
        {
            convex = newCollidableA as ConvexCollidable;
            mesh = newCollidableB as FullChunkObject;
            if (convex == null || mesh == null)
            {
                convex = newCollidableB as ConvexCollidable;
                mesh = newCollidableA as FullChunkObject;
                if (convex == null || mesh == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize contact manifold.");
                }
            }
            activePairs = new QuickDictionary<Vector3i, GeneralConvexPairTester>(BufferPools<Vector3i>.Locking, BufferPools<GeneralConvexPairTester>.Locking, BufferPools<int>.Locking, 3);
            activePairsBackBuffer = new QuickDictionary<Vector3i, GeneralConvexPairTester>(BufferPools<Vector3i>.Locking, BufferPools<GeneralConvexPairTester>.Locking, BufferPools<int>.Locking, 3);
        }

        public FCOContactManifold()
        {
            contacts = new RawList<Contact>(4);
            unusedContacts = new UnsafeResourcePool<Contact>(4);
            contactIndicesToRemove = new RawList<int>(4);
        }

        public RawList<Contact> ctcts
        {
            get
            {
                return contacts;
            }
        }

        private GeneralConvexPairTester GetPair(ref Vector3i position)
        {
            var pair = testerPool.Take();
            var boxCollidable = boxCollidablePool.Take();
            boxCollidable.Shape.Width = 1;
            boxCollidable.Shape.Height = 1;
            boxCollidable.Shape.Length = 1;
            pair.Initialize(convex, boxCollidable);
            boxCollidable.WorldTransform = new RigidTransform(new Vector3(
                mesh.Position.X + position.X,
                mesh.Position.Y + position.Y,
                mesh.Position.Z + position.Z));
            return pair;
        }
        
        private void ReturnPair(GeneralConvexPairTester pair)
        {
            boxCollidablePool.GiveBack((ReusableBoxCollidable)pair.CollidableB);
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
            var transform = new RigidTransform(mesh.Position);
            var convexTransform = convex.WorldTransform;
            ContactRefresher.ContactRefresh(contacts, supplementData, ref convexTransform, ref transform, contactIndicesToRemove);
            RemoveQueuedContacts();
            var overlaps = new QuickList<Vector3i>(BufferPools<Vector3i>.Thread);
            mesh.ChunkShape.GetOverlaps(mesh.Position, convex.BoundingBox, ref overlaps);
            var candidatesToAdd = new QuickList<ContactData>(BufferPools<ContactData>.Thread, BufferPool<int>.GetPoolIndex(overlaps.Count));
            for (int i = 0; i < overlaps.Count; ++i)
            {
                GeneralConvexPairTester manifold;
                if (!activePairs.TryGetValue(overlaps.Elements[i], out manifold))
                {
                    manifold = GetPair(ref overlaps.Elements[i]);
                }
                else
                {
                    activePairs.FastRemove(overlaps.Elements[i]);
                }
                activePairsBackBuffer.Add(overlaps.Elements[i], manifold);
                ContactData contactCandidate;
                if (manifold.GenerateContactCandidate(out contactCandidate))
                {
                    candidatesToAdd.Add(ref contactCandidate);
                }
            }
            overlaps.Dispose();
            for (int i = activePairs.Count - 1; i >= 0; --i)
            {
                ReturnPair(activePairs.Values[i]);
                activePairs.FastRemove(activePairs.Keys[i]);
            }
            var temp = activePairs;
            activePairs = activePairsBackBuffer;
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
            var convexTransform = convex.WorldTransform;
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
            convex = null;
            mesh = null;
            for (int i = activePairs.Count - 1; i >= 0; --i)
            {
                ReturnPair(activePairs.Values[i]);
                activePairs.Values[i].CleanUp();
            }
            activePairs.Clear();
            activePairs.Dispose();
            activePairsBackBuffer.Dispose();
            base.CleanUp();
        }
    }
}
