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
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;

namespace Voxalia.Shared.Collision
{
    public class MobileChunkCollidable : EntityCollidable
    {
        public MobileChunkCollidable(MobileChunkShape shape)
        {
            ChunkShape = shape;
            base.Shape = ChunkShape;
            Vector3 max = new Vector3(shape.ChunkSize.X, shape.ChunkSize.Y, shape.ChunkSize.Z);
            boundingBox = new BoundingBox(-max, max);
            Events = new ContactEventManager<EntityCollidable>();
        }

        public MobileChunkCollidable(Vector3i size, BlockInternal[] blocks)
        {
            ChunkShape = new MobileChunkShape(size, blocks);
            base.Shape = ChunkShape;
            Vector3 max = new Vector3(ChunkShape.ChunkSize.X, ChunkShape.ChunkSize.Y, ChunkShape.ChunkSize.Z);
            boundingBox = new BoundingBox(-max, max);
            Events = new ContactEventManager<EntityCollidable>();
        }

        public MobileChunkShape ChunkShape;
        
        protected override IContactEventTriggerer EventTriggerer
        {
            get { return Events; }
        }
        
        public bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweepnorm, float slen, MaterialSolidity solidness, out RayHit hit)
        {
            // TODO: Handle orientation!
            RigidTransform rt = new RigidTransform(startingTransform.Position - worldTransform.Position, startingTransform.Orientation);
            RayHit rHit;
            bool h = ChunkShape.ConvexCast(castShape, ref rt, ref sweepnorm, slen, solidness, out rHit);
            rHit.Location = rHit.Location + worldTransform.Position;
            hit = rHit;
            return h;
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, Func<BroadPhaseEntry, bool> filter, out RayHit hit)
        {
            // TODO: Handle orientation!
            RigidTransform rt = new RigidTransform(startingTransform.Position - worldTransform.Position, startingTransform.Orientation);
            RayHit rHit;
            float slen = sweep.Length();
            Vector3 sweepnorm = sweep / slen;
            bool h = ChunkShape.ConvexCast(castShape, ref rt, ref sweepnorm, slen, MaterialSolidity.FULLSOLID, out rHit);
            rHit.Location = rHit.Location + worldTransform.Position;
            hit = rHit;
            return h;
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, out RayHit hit)
        {
            return ConvexCast(castShape, ref startingTransform, ref sweep, null, out hit);
        }

        public bool RayCast(Ray ray, float maximumLength, Func<BroadPhaseEntry, bool> filter, MaterialSolidity solidness, out RayHit rayHit)
        {
            // TODO: Handle orientation!
            Ray r2 = new Ray(ray.Position - worldTransform.Position, ray.Direction);
            RayHit rHit;
            bool h = ChunkShape.RayCast(ref r2, maximumLength, solidness, out rHit);
            rHit.Location = rHit.Location + worldTransform.Position;
            rayHit = rHit;
            return h;
        }

        public override bool RayCast(Ray ray, float maximumLength, Func<BroadPhaseEntry, bool> filter, out RayHit rayHit)
        {
            return RayCast(ray, maximumLength, filter, MaterialSolidity.FULLSOLID, out rayHit);
        }

        public override bool RayCast(Ray ray, float maximumLength, out RayHit rayHit)
        {
            return RayCast(ray, maximumLength, null, out rayHit);
        }

        protected override void UpdateBoundingBoxInternal(float dt)
        {
            Shape.GetBoundingBox(ref worldTransform, out boundingBox);
            ExpandBoundingBox(ref boundingBox, dt);
        }
    }
}
