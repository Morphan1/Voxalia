using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;

namespace Voxalia.Shared.Collision
{
    public class ReusableGenericCollidable<T> : ConvexCollidable<T> where T: ConvexShape
    {
        public ReusableGenericCollidable(T tshape)
        {
            shape = tshape;
        }

        protected override void UpdateBoundingBoxInternal(double dt)
        {
            Shape.GetBoundingBox(ref worldTransform, out boundingBox);
            ExpandBoundingBox(ref boundingBox, dt);
        }

        public void SetEntity(Entity e)
        {
            Entity = e;
        }
    }
}
