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
        protected ConvexCollidable convex;

        protected FullChunkObject mesh;

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
        }

        public FCOContactManifold()
        {
            contacts = new RawList<Contact>();
            unusedContacts = new UnsafeResourcePool<Contact>(4);
            contactIndicesToRemove = new RawList<int>();
        }

        public RawList<Contact> ctcts
        {
            get
            {
                return contacts;
            }
        }

        public static bool IsNaNOrInfOrZero(ref Vector3 vec)
        {
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X)
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y)
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z) || (vec.X == 0 && vec.Y == 0 && vec.Z == 0);
        }

        public static bool IsNaNOrInf(ref Vector3 vec)
        {
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X)
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y)
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z);
        }
        
        public override void Update(float dt)
        {

            RigidTransform rt = convex.Entity == null ? convex.WorldTransform: new RigidTransform(convex.Entity.Position, convex.Entity.Orientation);
            if (IsNaNOrInf(ref rt.Position))
            {
                for (int i = contacts.Count - 1; i >= 0; i--)
                {
                    Remove(i);
                }
                return;
            }
            Vector3 sw = new Vector3(0, 0, 1f);
            float len = 1;
            if (convex.Entity != null) // NOTE: Wot?
            {
                sw = convex.Entity.LinearVelocity;
                len = sw.Length();
                sw /= len;
            }
            RayHit rh;
            bool hit = mesh.ConvexCast(convex.Shape, ref rt, ref sw, len * dt, MaterialSolidity.FULLSOLID, out rh);
            if (!hit || IsNaNOrInfOrZero(ref rh.Normal))
            {
                for (int i = contacts.Count - 1; i >= 0; i--)
                {
                    Remove(i);
                }
                return;
            }
            Location tnorm = new Location(rh.Normal).Normalize();
            #region MathIsWierd
            Location rando = new Location(1, 0, 0);
            if (tnorm == rando)
            {
                rando = new Location(0, 1, 0);
            }
            Location axisone = (rando - (rando.Dot(tnorm) * tnorm)).Normalize();
            Location axistwo = tnorm.CrossProduct(axisone);
            #endregion
            BoundingBox bb = new BoundingBox(rh.Location - axisone.ToBVector() - axistwo.ToBVector(), rh.Location + axisone.ToBVector() + axistwo.ToBVector());
            Ray ray = new Ray(rt.Position, sw);
            float t;
            Vector3 pos = rh.Location;
            if (ray.Intersects(bb, out t))
            {
                ray.GetPointOnRay(t, out pos);
            }
            float pendef = Math.Abs(len * dt - t);
            Vector3 norm;
            RigidTransform rtx = new RigidTransform(Vector3.Zero, rt.Orientation);
            RigidTransform.Transform(ref rh.Normal, ref rtx, out norm);
            norm = -norm; // TODO: Why must we negate here?!
            for (int i = contacts.Count - 1; i >= 0; i--)
            {
                contacts[i].Normal = norm;
                contacts[i].Position = pos;
                contacts[i].PenetrationDepth = pendef;
            }
            if (Contacts.Count == 0)
            {
                ContactData cd = new ContactData();
                cd.Normal = norm;
                cd.Position = pos;
                cd.PenetrationDepth = pendef;
                cd.Id = contacts.Count;
                Add(ref cd);
            }
        }
        
        public override void CleanUp()
        {
            convex = null;
            mesh = null;
            base.CleanUp();
        }
    }
}
