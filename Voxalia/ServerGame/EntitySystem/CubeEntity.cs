using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class CubeEntity: CuboidalEntity
    {
        public CubeEntity(Location half, Region tregion, float mass)
            : base(half, tregion, true, mass)
        {
            NetworkMe = true;
        }

        bool pActive = false;

        public double deltat = 0;

        public Location pVelocity;

        public override void Tick()
        {
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
            if (!Body.ActivityInformation.IsActive && GetMass() <= 0 && GetVelocity() != pVelocity)
            {
                pVelocity = GetVelocity();
                TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
            }
            base.Tick();
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "textures":
                    Textures = data.Split('|');
                    if (Textures.Length != 6)
                    {
                        DefTexs();
                    }
                    return true;
                case "coords":
                    TexCoords = data.Split('|');
                    if (TexCoords.Length != 6)
                    {
                        DefTexs();
                    }
                    return true;
                case "water":
                    if (data.ToLower() == "true")
                    {
                        CGroup = CollisionUtil.Water;
                    }
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("textures", TexString()));
            vars.Add(new KeyValuePair<string, string>("coords", TexCString()));
            vars.Add(new KeyValuePair<string, string>("water", CGroup == CollisionUtil.Water ? "true" : "false"));
            return vars;
        }

        public override string ToString()
        {
            return "CUBEENTITY{mins=" + mins + ",maxes=" + maxes + ",mass=" + GetMass() + "}";
        }
    }
}
