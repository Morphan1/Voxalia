using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;

namespace ShadowOperations.ServerGame.EntitySystem
{
    class CubeEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(1);

        public string Textures = "top|bottom|xp|xm|yp|ym";
        public string TexCoords = "1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f";

        public CubeEntity(Location half, Server tserver, float mass)
            : base(tserver, true)
        {
            HalfSize = half;
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            SetMass(mass);
            mins = -HalfSize;
            maxes = HalfSize;
        }

        bool pActive = false;

        public override void Tick()
        {
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                PhysicsEntityUpdatePacketOut peupo = new PhysicsEntityUpdatePacketOut(this);
                for (int i = 0; i < TheServer.Players.Count; i++)
                {
                    TheServer.Players[i].Network.SendPacket(peupo);
                }
            }
        }

        Location mins;
        Location maxes;

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "mins":
                    mins = Location.FromString(data);
                    return true;
                case "maxes":
                    maxes = Location.FromString(data);
                    return true;
                case "textures":
                    Textures = data;
                    return true;
                case "coords":
                    TexCoords = data;
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("mins", mins.ToString()));
            vars.Add(new KeyValuePair<string, string>("maxes", maxes.ToString()));
            vars.Add(new KeyValuePair<string, string>("textures", Textures));
            vars.Add(new KeyValuePair<string, string>("coords", TexCoords));
            return vars;
        }

        public override void Recalculate()
        {
            HalfSize = (maxes - mins) / 2;
            SetPosition(mins + HalfSize);
        }
    }
}
