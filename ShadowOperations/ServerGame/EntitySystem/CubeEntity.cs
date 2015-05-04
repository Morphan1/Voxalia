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

        public string[] Textures;
        public string[] TexCoords;

        void DefTexs()
        {
            Textures = "top|bottom|xp|xm|yp|ym".Split('|');
            TexCoords = "1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f".Split('|');
        }

        public CubeEntity(Location half, Server tserver, float mass)
            : base(tserver, true)
        {
            DefTexs();
            HalfSize = half;
            SetMass(mass);
            BuildShape();
            mins = -HalfSize;
            maxes = HalfSize;
        }

        void BuildShape()
        {
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Widest = (float)HalfSize.Length();
        }

        bool pActive = false;

        public double deltat = 0;

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
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("mins", mins.ToString()));
            vars.Add(new KeyValuePair<string, string>("maxes", maxes.ToString()));
            vars.Add(new KeyValuePair<string, string>("textures", TexString()));
            vars.Add(new KeyValuePair<string, string>("coords", TexCString()));
            return vars;
        }

        public string TexString()
        {
            return Textures[0] + "|" + Textures[1] + "|" + Textures[2] + "|" + Textures[3] + "|" + Textures[4] + "|" + Textures[5];
        }

        public string TexCString()
        {
            return TexCoords[0] + "|" + TexCoords[1] + "|" + TexCoords[2] + "|" + TexCoords[3] + "|" + TexCoords[4] + "|" + TexCoords[5];
        }

        public override void Recalculate()
        {
            HalfSize = (maxes - mins) / 2;
            SetPosition(mins + HalfSize);
            BuildShape();
        }

        public override string ToString()
        {
            return "CUBEENTITY{mins=" + mins + ",maxes=" + maxes + ",mass=" + GetMass() + "}";
        }
    }
}
