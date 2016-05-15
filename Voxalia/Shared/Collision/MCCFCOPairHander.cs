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
    public class MCCFCOPairHandler : GroupPairHandler
    {
        FullChunkObject mesh;

        MobileChunkCollidable mobile;
        
        public override Collidable CollidableA
        {
            get { return mobile; }
        }

        public override Collidable CollidableB
        {
            get { return mesh; }
        }

        public override Entity EntityA
        {
            get { return mobile.Entity; }
        }

        public override Entity EntityB
        {
            get { return null; }
        }
        
        public MCCFCOPairHandler()
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
            mesh = entryA as FullChunkObject;
            mobile = entryB as MobileChunkCollidable;
            if (mesh == null || mobile == null)
            {
                mesh = entryB as FullChunkObject;
                mobile = entryA as MobileChunkCollidable;
                if (mesh == null || mobile == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }
            broadPhaseOverlap = new BroadPhaseOverlap(mobile, mesh, broadPhaseOverlap.CollisionRule);
            UpdateMaterialProperties(mobile.Entity != null ? mobile.Entity.Material : null, mesh.Material);
            base.Initialize(entryA, entryB);
            noRecurse = false;
        }
        
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            mobile = null;
        }
        
        protected override void UpdateContainedPairs()
        {
            RigidTransform rtMesh = new RigidTransform(mesh.Position);
            RigidTransform rtMobile = mobile.WorldTransform;
            QuickList<Vector3i> overlaps = new QuickList<Vector3i>(BufferPools<Vector3i>.Thread);
            mesh.ChunkShape.GetOverlaps(mesh.Position, mobile.BoundingBox, ref overlaps);
            for (int i = 0; i < overlaps.Count; i++)
            {
                Vector3i pos = overlaps.Elements[i];
                Vector3 offs;
                ReusableGenericCollidable<ConvexShape> colBox = new ReusableGenericCollidable<ConvexShape>(mesh.ChunkShape.ShapeAt(pos.X, pos.Y, pos.Z, out offs));
                Vector3 input = new Vector3(pos.X + offs.X, pos.Y + offs.Y, pos.Z + offs.Z);
                RigidTransform outp = new RigidTransform(input + mesh.Position);
                colBox.WorldTransform = outp;
                colBox.UpdateBoundingBoxForTransform(ref outp);
                QuickList<Vector3i> overlaps2 = new QuickList<Vector3i>(BufferPools<Vector3i>.Thread);
                mobile.ChunkShape.GetOverlaps(ref rtMobile, colBox.BoundingBox, ref overlaps2);
                for (int x = 0; x < overlaps2.Count; x++)
                {
                    Vector3i pos2 = overlaps2.Elements[x];
                    Vector3 offs2;
                    ReusableGenericCollidable<ConvexShape> colBox2 = new ReusableGenericCollidable<ConvexShape>(mobile.ChunkShape.ShapeAt(pos2.X, pos2.Y, pos2.Z, out offs2));
                    colBox2.SetEntity(mobile.Entity);
                    Vector3 input2 = new Vector3(pos2.X + offs2.X, pos2.Y + offs2.Y, pos2.Z + offs2.Z);
                    Vector3 transfd2 = Quaternion.Transform(input2, rtMobile.Orientation);
                    RigidTransform outp2 = new RigidTransform(transfd2 + rtMobile.Position, rtMobile.Orientation);
                    colBox2.WorldTransform = outp2;
                    TryToAdd(colBox, colBox2, mesh.Material, mobile.Entity != null ? mobile.Entity.Material : null);
                }
                overlaps2.Dispose();
            }
            overlaps.Dispose();
        }
    }
}
