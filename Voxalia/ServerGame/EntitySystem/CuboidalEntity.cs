using System.Collections.Generic;
using Voxalia.Shared;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class CuboidalEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(1);

        public string[] Textures;
        public string[] TexCoords;

        public void DefTexs()
        {
            Textures = "top|bottom|xp|xm|yp|ym".Split('|');
            TexCoords = "1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f".Split('|');
        }

        public CuboidalEntity(Location half, Region tregion, bool ticks, float mass)
            : base(tregion, ticks)
        {
            DefTexs();
            HalfSize = half;
            SetMass(mass);
            BuildShape();
            mins = -HalfSize;
            maxes = HalfSize;
        }

        public void BuildShape()
        {
            Shape = new BoxShape((float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Widest = (float)HalfSize.Length();
        }

        public Location mins;
        public Location maxes;

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
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("mins", mins.ToString()));
            vars.Add(new KeyValuePair<string, string>("maxes", maxes.ToString()));
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

    }
}
