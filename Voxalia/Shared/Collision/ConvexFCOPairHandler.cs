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

namespace Voxalia.Shared.Collision
{
    public class ConvexFCOPairHandler : StandardPairHandler
    {
        FullChunkObject mesh;

        ConvexCollidable convex;

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
            get { return contactManifold; }
        }

        FCOContactManifold contactManifold = new FCOContactManifold();
        
        public ConvexFCOPairHandler()
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
            convex = entryB as ConvexCollidable;
            if (mesh == null || convex == null)
            {
                mesh = entryB as FullChunkObject;
                convex = entryA as ConvexCollidable;
                if (mesh == null || convex == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }
            BroadPhaseOverlap = new BEPUphysics.BroadPhaseSystems.BroadPhaseOverlap(convex, mesh);
            UpdateMaterialProperties(convex.Entity != null ? convex.Entity.Material : null, mesh.Material);
            base.Initialize(entryA, entryB);
            InteractionProperties ip = contactConstraint.MaterialInteraction;
            ip.StaticFriction = 0f;
            ip.KineticFriction = 0f;
            ip.Bounciness = 0.5f;
            contactConstraint.MaterialInteraction = ip;
            noRecurse = false;
        }
        
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
                RigidTransform rt = new RigidTransform(convex.Entity.Position, convex.Entity.Orientation);
                Vector3 sweep = convex.Entity.LinearVelocity;
                sweep *= dt;
                RayHit rh;
                if (mesh.ConvexCast(convex.Shape, ref rt, ref sweep, out rh))
                {
                    timeOfImpact = rh.T;
                }
                if (TimeOfImpact < 0)
                {
                    timeOfImpact = 0;
                }
            }
        }
        
        protected override void GetContactInformation(int index, out ContactInformation info)
        {
            ContactInformation ci = new ContactInformation();
            ci.Contact = contactManifold.ctcts[index];
            ci.Pair = this;
            ReadOnlyList<ContactPenetrationConstraint> list = contactConstraint.ContactPenetrationConstraints;
            float totalimp = 0;
            for (int i = 0; i < list.Count; i++)
            {
                totalimp += list[i].NormalImpulse;
            }
            ci.NormalImpulse = list[index].NormalImpulse;
            ci.FrictionImpulse = (ci.NormalImpulse / totalimp) * list[index].RelativeVelocity;
            if (convex.Entity != null)
            {
                Vector3 velocity;
                Vector3 cep = convex.Entity.Position;
                Vector3 ceav = convex.Entity.AngularVelocity;
                Vector3 celv = convex.Entity.LinearVelocity;
                Vector3.Subtract(ref ci.Contact.Position, ref cep, out velocity);
                Vector3.Cross(ref ceav, ref velocity, out velocity);
                Vector3.Add(ref velocity, ref celv, out ci.RelativeVelocity);
            }
            else
            {
                ci.RelativeVelocity = new Vector3(0, 0, 0);
            }
            info = ci;
        }
    }
}
