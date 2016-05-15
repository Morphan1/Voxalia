using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionTests.CollisionAlgorithms.GJK;
using BEPUphysics.Constraints.Collision;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Entities;
using BEPUutilities;
using BEPUphysics;
using BEPUutilities.DataStructures;
using BEPUphysics.Materials;
using BEPUphysics.BroadPhaseSystems;
using BEPUutilities.ResourceManagement;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace Voxalia.Shared.Collision
{
    public class ConvexMCCPairHandler : GroupPairHandler
    {
        MobileChunkCollidable mesh;

        ConvexCollidable convex;
        
        public override Collidable CollidableA
        {
            get { return convex; }
        }

        public override Collidable CollidableB
        {
            get { return mesh; }
        }

        public override Entity EntityA
        {
            get { return convex.Entity; }
        }

        public override Entity EntityB
        {
            get { return null; }
        }
        
        public ConvexMCCPairHandler()
        {
        }

        bool noRecurse = false;
        
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {
            if (noRecurse)
            {
                return;
            }
            noRecurse = true;
            mesh = entryA as MobileChunkCollidable;
            convex = entryB as ConvexCollidable;
            if (mesh == null || convex == null)
            {
                mesh = entryB as MobileChunkCollidable;
                convex = entryA as ConvexCollidable;
                if (mesh == null || convex == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }
            broadPhaseOverlap = new BroadPhaseOverlap(convex, mesh, broadPhaseOverlap.CollisionRule);
            UpdateMaterialProperties(convex.Entity != null ? convex.Entity.Material : null, mesh.Entity != null ? mesh.Entity.Material: null);
            base.Initialize(entryA, entryB);
            noRecurse = false;
        }
        
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            convex = null;
        }

        protected override void UpdateContainedPairs()
        {
            RigidTransform rt = mesh.WorldTransform;
            QuickList<Vector3i> overlaps = new QuickList<Vector3i>(BufferPools<Vector3i>.Thread);
            mesh.ChunkShape.GetOverlaps(ref rt, convex.BoundingBox, ref overlaps);
            for (int i = 0; i < overlaps.Count; i++)
            {
                Vector3i pos = overlaps.Elements[i];
                Vector3 offs;
                ReusableGenericCollidable<ConvexShape> colBox = new ReusableGenericCollidable<ConvexShape>(mesh.ChunkShape.ShapeAt(pos.X, pos.Y, pos.Z, out offs));
                colBox.SetEntity(mesh.Entity);
                Vector3 input = new Vector3(pos.X + offs.X, pos.Y + offs.Y, pos.Z + offs.Z);
                Vector3 transfd = Quaternion.Transform(input, rt.Orientation);
                RigidTransform outp = new RigidTransform(transfd + rt.Position, rt.Orientation);
                colBox.WorldTransform = outp;
                TryToAdd(colBox, convex, mesh.Entity != null ? mesh.Entity.Material : null, convex.Entity != null ? convex.Entity.Material : null);
            }
            overlaps.Dispose();
        }
    }
}
