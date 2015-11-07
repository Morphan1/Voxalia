using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries.Events;
using BEPUphysics.OtherSpaceStages;
using BEPUphysics.NarrowPhaseSystems;
using BEPUphysics.CollisionShapes;
using BEPUphysics.BroadPhaseSystems;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionRuleManagement;
using BEPUutilities.DataStructures;
using System;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.Constraints.Collision;
using BEPUphysics.Entities;
using BEPUphysics.CollisionTests.CollisionAlgorithms.GJK;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;

namespace Voxalia.Shared.Collision
{
    public class FullChunkObject : StaticCollidable
    {
        public static void RegisterMe()
        {
            NarrowPhasePairFactory<ConvexFCOPairHandler> fact = new NarrowPhasePairFactory<ConvexFCOPairHandler>();
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<BoxShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<SphereShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<CapsuleShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<TriangleShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<CylinderShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<ConeShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<TransformableShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<MinkowskiSumShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<WrappedShape>), typeof(FullChunkObject)), fact);
            NarrowPhaseHelper.CollisionManagers.Add(new TypePair(typeof(ConvexCollidable<ConvexHullShape>), typeof(FullChunkObject)), fact);
        }

        public FullChunkObject(Vector3 pos, BlockInternal[] blocks)
        {
            ChunkShape = new FullChunkShape(blocks);
            Position = pos;
            boundingBox = new BoundingBox(Position, Position + new Vector3(30, 30, 30));
        }

        public FullChunkShape ChunkShape;

        public Vector3 Position;

        public ContactEventManager<FullChunkObject> Events = new ContactEventManager<FullChunkObject>();

        protected override IContactEventTriggerer EventTriggerer
        {
            get { return Events; }
        }

        protected override IDeferredEventCreator EventCreator
        {
            get { return Events; }
        }

        public override void UpdateBoundingBox()
        {
            boundingBox = new BoundingBox(Position, Position + new Vector3(30, 30, 30));
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, Func<BroadPhaseEntry, bool> filter, out RayHit hit)
        {
            RigidTransform rt = new RigidTransform(startingTransform.Position - Position, startingTransform.Orientation);
            RayHit rHit;
            float slen = sweep.Length();
            Vector3 sweepnorm = sweep / slen;
            bool h = ChunkShape.ConvexCast(castShape, ref rt, ref sweepnorm, slen, out rHit);
            rHit.Location = rHit.Location + Position;
            hit = rHit;
            return h;
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, out RayHit hit)
        {
            return ConvexCast(castShape, ref startingTransform, ref sweep, null, out hit);
        }

        public override bool RayCast(Ray ray, float maximumLength, Func<BroadPhaseEntry, bool> filter, out RayHit rayHit)
        {
            Ray r2 = new Ray(ray.Position - Position, ray.Direction);
            RayHit rHit;
            bool h = ChunkShape.RayCast(ref r2, maximumLength, out rHit);
            rHit.Location = rHit.Location + Position;
            rayHit = rHit;
            return h;
        }

        public override bool RayCast(Ray ray, float maximumLength, out RayHit rayHit)
        {
            return RayCast(ray, maximumLength, null, out rayHit);
        }
    }
}
