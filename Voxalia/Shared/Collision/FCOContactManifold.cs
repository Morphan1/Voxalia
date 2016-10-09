//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
        static LockingResourcePool<GeneralConvexPairTester> testerPool = new LockingResourcePool<GeneralConvexPairTester>();

        protected ConvexCollidable convex;

        protected FullChunkObject mesh;

        public QuickDictionary<Vector3i, GeneralConvexPairTester> ActivePairs;
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
            ActivePairs = new QuickDictionary<Vector3i, GeneralConvexPairTester>(BufferPools<Vector3i>.Locking, BufferPools<GeneralConvexPairTester>.Locking, BufferPools<int>.Locking, 3);
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
            // TODO: Efficiency!
            GeneralConvexPairTester pair = testerPool.Take();
            Vector3 offs;
            ReusableGenericCollidable<ConvexShape> boxCollidable = new ReusableGenericCollidable<ConvexShape>((ConvexShape)mesh.ChunkShape.ShapeAt(position.X, position.Y, position.Z, out offs));
            pair.Initialize(convex, boxCollidable);
            boxCollidable.WorldTransform = new RigidTransform(new Vector3(
                mesh.Position.X + position.X + offs.X,
                mesh.Position.Y + position.Y + offs.Y,
                mesh.Position.Z + position.Z + offs.Z));
            return pair;
        }
        
        private void ReturnPair(GeneralConvexPairTester pair)
        {
            pair.CleanUp();
            testerPool.GiveBack(pair);
        }

        public static bool IsNaNOrInfOrZero(ref Vector3 vec)
        {
            return double.IsInfinity(vec.X) || double.IsNaN(vec.X)
                || double.IsInfinity(vec.Y) || double.IsNaN(vec.Y)
                || double.IsInfinity(vec.Z) || double.IsNaN(vec.Z) || (vec.X == 0 && vec.Y == 0 && vec.Z == 0);
        }

        public static bool IsNaNOrInf(ref Vector3 vec)
        {
            return double.IsInfinity(vec.X) || double.IsNaN(vec.X)
                || double.IsInfinity(vec.Y) || double.IsNaN(vec.Y)
                || double.IsInfinity(vec.Z) || double.IsNaN(vec.Z);
        }
        
        public override void Update(double dt)
        {
            RigidTransform transform = new RigidTransform(mesh.Position);
            RigidTransform convexTransform = convex.WorldTransform;
            ContactRefresher.ContactRefresh(contacts, supplementData, ref convexTransform, ref transform, contactIndicesToRemove);
            RemoveQueuedContacts();
            var overlaps = new QuickList<Vector3i>(BufferPools<Vector3i>.Thread);
            mesh.ChunkShape.GetOverlaps(mesh.Position, convex.BoundingBox, ref overlaps);
            var candidatesToAdd = new QuickList<ContactData>(BufferPools<ContactData>.Thread, BufferPool<int>.GetPoolIndex(overlaps.Count));
            for (int i = 0; i < overlaps.Count; i++)
            {
                GeneralConvexPairTester manifold;
                if (!ActivePairs.TryGetValue(overlaps.Elements[i], out manifold))
                {
                    manifold = GetPair(ref overlaps.Elements[i]);
                }
                else
                {
                    ActivePairs.FastRemove(overlaps.Elements[i]);
                }
                activePairsBackBuffer.Add(overlaps.Elements[i], manifold);
                ContactData contactCandidate;
                if (manifold.GenerateContactCandidate(out contactCandidate))
                {
                    candidatesToAdd.Add(ref contactCandidate);
                }
            }
            overlaps.Dispose();
            for (int i = ActivePairs.Count - 1; i >= 0; i--)
            {
                ReturnPair(ActivePairs.Values[i]);
                ActivePairs.FastRemove(ActivePairs.Keys[i]);
            }
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
            RigidTransform convexTransform = convex.WorldTransform;
            RigidTransform gridTransform = new RigidTransform(mesh.Position);
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
            for (int i = ActivePairs.Count - 1; i >= 0; --i)
            {
                ReturnPair(ActivePairs.Values[i]);
                ActivePairs.Values[i].CleanUp();
            }
            ActivePairs.Clear();
            ActivePairs.Dispose();
            activePairsBackBuffer.Dispose();
            base.CleanUp();
        }
    }
}
