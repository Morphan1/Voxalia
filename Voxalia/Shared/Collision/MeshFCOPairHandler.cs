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

namespace Voxalia.Shared.Collision
{
    public class MeshFCOPairHandler : StandardPairHandler
    {
        FullChunkObject mesh;

        MobileMeshCollidable convex;

        private NonConvexContactManifoldConstraint contactConstraint;
        
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

        public override ContactManifoldConstraint ContactConstraint
        {
            get { return contactConstraint; }
        }

        public override ContactManifold ContactManifold
        {
            get { return MeshManifold; }
        }

        FCOContactManifold contactManifold = new FCOContactManifold();

        protected FCOContactManifold MeshManifold { get { return contactManifold; } }

        public MeshFCOPairHandler()
        {
            contactConstraint = new NonConvexContactManifoldConstraint(this);
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
            convex = entryB as MobileMeshCollidable;
            if (mesh == null || convex == null)
            {
                mesh = entryB as FullChunkObject;
                convex = entryA as MobileMeshCollidable;
                if (mesh == null || convex == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }
            //Contact normal goes from A to B.
            BroadPhaseOverlap = new BEPUphysics.BroadPhaseSystems.BroadPhaseOverlap(convex, mesh);
            UpdateMaterialProperties(convex.Entity != null ? convex.Entity.Material : null, mesh.Material);
            base.Initialize(entryA, entryB);
            noRecurse = false;
        }
        
        ///<summary>
        /// Cleans up the pair handler.
        ///</summary>
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            convex = null;
        }
        
        public override void UpdateTimeOfImpact(Collidable requester, float dt)
        {
            if (convex.Entity != null && convex.Entity.ActivityInformation.IsActive && convex.Entity.PositionUpdateMode == PositionUpdateMode.Continuous)
            {
                timeOfImpact = 1;
                // TODO!
                timeOfImpact = 0;
                /*RigidTransform rt = new RigidTransform(convex.Entity.Position, convex.Entity.Orientation);
                Vector3 sweep = convex.Entity.LinearVelocity;
                RayHit rh;
                if (mesh.ConvexCast(convex.Shape, ref rt, ref sweep, out rh))
                {
                    timeOfImpact = rh.T;
                }*/
                // Special exception!
                if (TimeOfImpact <= 0)
                {
                    timeOfImpact = 1;
                }
            }
        }
        
        protected override void GetContactInformation(int index, out ContactInformation info)
        {
            ContactInformation ci = new ContactInformation();
            ci.Contact = contactManifold.ctcts[index];
            ci.Pair = this;
            ci.NormalImpulse = -100; // convex.Entity.Mass * 10f;
            ci.FrictionImpulse = -1; // convex.Entity.Mass;
            info = ci;
        }
    }
}
