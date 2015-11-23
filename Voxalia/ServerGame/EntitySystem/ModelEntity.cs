using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.OtherSystems;

namespace Voxalia.ServerGame.EntitySystem
{
    public class ModelEntity: PhysicsEntity
    {
        public string model;

        public Location scale = Location.One;

        public ModelEntity(string mod, Region tregion)
            : base(tregion, true)
        {
            model = mod;
        }

        bool pActive = false;

        public double deltat = 0;

        public ModelCollisionMode mode = ModelCollisionMode.AABB;

        public override void Tick()
        {
            if (Body == null)
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
            }
            if (!pActive && GetMass() > 0)
            {
                deltat += TheRegion.Delta;
                if (deltat > 2.0)
                {
                    TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
                }
            }
            base.Tick();
        }
        
        public Location offset;

        public override void SpawnBody()
        {
            Model smod = TheServer.Models.GetModel(model);
            if (smod == null) // TODO: smod should return a cube when all else fails?
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            Model3D smodel = smod.Original;
            if (smodel == null) // TODO: smodel should return a cube when all else fails?
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            if (mode == ModelCollisionMode.PRECISE)
            {
                Shape = TheServer.Models.handler.MeshToBepu(smodel); // TODO: Scale!
            }
            else if (mode == ModelCollisionMode.AABB)
            {
                List<BEPUutilities.Vector3> vecs = TheServer.Models.handler.GetCollisionVertices(smodel);
                Location zero = new Location(vecs[0]);
                AABB abox = new AABB() { Min = zero, Max = zero };
                for (int v = 1; v < vecs.Count; v++)
                {
                    abox.Include(new Location(vecs[v]));
                }
                Location size = abox.Max - abox.Min;
                Location center = abox.Max - size / 2;
                offset = -center;
                Shape = new BoxShape((float)size.X * (float)scale.X, (float)size.Y * (float)scale.Y, (float)size.Z * (float)scale.Z);
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = TheServer.Models.handler.GetCollisionVertices(smodel);
                Location zero = new Location(vecs[0].X, vecs[0].Y, vecs[0].Z);
                double distSq = 0;
                for (int v = 1; v < vecs.Count; v++)
                {
                    if (vecs[v].LengthSquared() > distSq)
                    {
                        distSq = vecs[v].LengthSquared();
                    }
                }
                double size = Math.Sqrt(distSq);
                offset = Location.Zero;
                Shape = new SphereShape((float)size * (float)scale.Length());
            }
            base.SpawnBody();
            if (mode == ModelCollisionMode.PRECISE)
            {
                offset = InternalOffset;
            }
        }
    }

    public enum ModelCollisionMode : byte
    {
        PRECISE = 1,
        AABB = 2,
        SPHERE = 3
        // TODO: ConvexHull!
    }
}
