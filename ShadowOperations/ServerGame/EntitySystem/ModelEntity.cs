using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.EntitySystem
{
    class ModelEntity: PhysicsEntity
    {
        public string model;

        public Location scale = Location.One;

        public ModelEntity(string mod, Server tserver)
            : base(tserver, true)
        {
            model = mod;
        }

        bool pActive = false;

        public double deltat = 0;

        public ModelCollisionMode mode = ModelCollisionMode.AABB;

        public override void Tick()
        {
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                TheServer.SendToAll(new PhysicsEntityUpdatePacketOut(this));
            }
            if (!pActive && GetMass() > 0)
            {
                deltat += TheServer.Delta;
                if (deltat > 2.0)
                {
                    TheServer.SendToAll(new PhysicsEntityUpdatePacketOut(this));
                }
            }
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "model":
                    model = value;
                    return true;
                case "scale":
                    scale = Location.FromString(value);
                    return true;
                case "collisionmode":
                    ModelCollisionMode newmode;
                    if (Enum.TryParse(value.ToUpper(), out newmode))
                    {
                        mode = newmode;
                    }
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("model", model));
            vars.Add(new KeyValuePair<string, string>("scale", scale.ToString()));
            vars.Add(new KeyValuePair<string, string>("collisionmode", mode.ToString().ToLower()));
            return vars;
        }

        public override void SpawnBody()
        {
            Assimp.Scene smodel = TheServer.Models.GetModel(model).Original;
            if (mode == ModelCollisionMode.PRECISE)
            {
                Shape = TheServer.Models.handler.MeshToBepu(smodel);
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = TheServer.Models.handler.GetCollisionVertices(smodel);
                Location zero = Location.FromBVector(vecs[0]);
                AABB abox = new AABB() { Min = zero, Max = zero };
                for (int v = 1; v < vecs.Count; v++)
                {
                    abox.Include(Location.FromBVector(vecs[v]));
                }
                Location size = abox.Max - abox.Min;
                Location center = abox.Max - size / 2;
                Shape = new BEPUphysics.Entities.Prefabs.Box(new BEPUphysics.EntityStateManagement.MotionState() { Position = BEPUutilities.Vector3.Zero,
                    Orientation = BEPUutilities.Quaternion.Identity }, (float)size.X, (float)size.Y, (float)size.Z);
            }
            base.SpawnBody();
        }
    }

    public enum ModelCollisionMode : byte
    {
        PRECISE = 1,
        AABB = 2
    }
}
