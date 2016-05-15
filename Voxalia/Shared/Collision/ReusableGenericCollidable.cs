using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

namespace Voxalia.Shared.Collision
{
    public class ReusableGenericCollidable<T> : ConvexCollidable<T> where T: ConvexShape
    {
        public ReusableGenericCollidable(T tshape)
            : base(tshape)
        {
        }

        protected override void UpdateBoundingBoxInternal(float dt)
        {
            Shape.GetBoundingBox(ref worldTransform, out boundingBox);
            ExpandBoundingBox(ref boundingBox, dt);
        }
    }
}
