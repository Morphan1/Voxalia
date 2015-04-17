using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using BulletSharp;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    class CubeEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(1);

        public string Textures = "top|bottom|xp|xm|yp|ym";
        public string TexCoords = "1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f";

        public CubeEntity(Location half, Server tserver, float mass)
            : base(tserver, false)
        {
            HalfSize = half;
            Shape = new BoxShape(HalfSize.ToBVector());
            SetMass(mass);
        }
    }
}
