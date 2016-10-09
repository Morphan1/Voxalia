//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
            LocalPosition = -shape.Center;
        }
        
        public MobileChunkShape ChunkShape;
        
        protected override IContactEventTriggerer EventTriggerer
        {
            get { return Events; }
        }
        
        public bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweepnorm, double slen, MaterialSolidity solidness, out RayHit hit)
        {
            RigidTransform rt;
            RigidTransform.MultiplyByInverse(ref startingTransform, ref worldTransform, out rt);
            Vector3 swp = Quaternion.Transform(sweepnorm, Quaternion.Inverse(worldTransform.Orientation));
            RayHit rh;
            bool h = ChunkShape.ConvexCast(castShape, ref rt, ref swp, slen, solidness, out rh);
            RigidTransform.Transform(ref rh.Location, ref worldTransform, out hit.Location);
            hit.Normal = rh.Normal;
            hit.T = rh.T;
            return h;
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, Func<BroadPhaseEntry, bool> filter, out RayHit hit)
        {
            Vector3 swp = sweep;
            double len = swp.Length();
            swp /= len;
            return ConvexCast(castShape, ref startingTransform, ref swp, len, MaterialSolidity.FULLSOLID, out hit);
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, out RayHit hit)
        {
            return ConvexCast(castShape, ref startingTransform, ref sweep, null, out hit);
        }
        
        public override bool RayCast(Ray ray, double maximumLength, Func<BroadPhaseEntry, bool> filter, out RayHit rayHit)
        {
            RigidTransform start = new RigidTransform(ray.Position);
            Vector3 sweep = ray.Direction;
            return ConvexCast(new BoxShape(0.1f, 0.1f, 0.1f), ref start, ref sweep, maximumLength, MaterialSolidity.FULLSOLID, out rayHit);
        }

        public override bool RayCast(Ray ray, double maximumLength, out RayHit rayHit)
        {
            return RayCast(ray, maximumLength, null, out rayHit);
        }

        protected override void UpdateBoundingBoxInternal(double dt)
        {
            Shape.GetBoundingBox(ref worldTransform, out boundingBox);
            ExpandBoundingBox(ref boundingBox, dt);
        }
    }
}
