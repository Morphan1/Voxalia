using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.GraphicsSystems.ParticleSystem
{
    public class ParticleHelper
    {
        Texture[] Explosion;

        Texture SmokeT;

        Texture White;

        Texture White_Blur;

        Client TheClient;

        public ParticleHelper(Client tclient)
        {
            TheClient = tclient;
            Explosion = new Texture[3];
            for (int i = 0; i < 3; i++)
            {
                Explosion[i] = TheClient.Textures.GetTexture("effects/explosion/0" + (i + 1));
            }
            White = TheClient.Textures.White;
            White_Blur = TheClient.Textures.GetTexture("common/white_blur");
            SmokeT = TheClient.Textures.GetTexture("effects/smoke/smoke1");
        }

        public void Sort()
        {
            Engine.ActiveEffects = Engine.ActiveEffects.OrderBy((o) => -(Engine.TheClient.CameraPos - o.Start(o)).LengthSquared()).ToList();
        }

        public ParticleEngine Engine;

        public void Explode(Location pos, float size, int dens = 50)
        {
            Location c1 = new Location(1, 0.7, 0);
            Location c2 = new Location(1);
            for (int i = 0; i < dens; i++)
            {
                Texture tex = Explosion[Utilities.UtilRandom.Next(Explosion.Length)];
                Location forward = Utilities.ForwardVector_Deg(Utilities.UtilRandom.NextDouble() * 360, Utilities.UtilRandom.NextDouble() * 360 - 180);
                double ssize = Utilities.UtilRandom.NextDouble() * 0.25 + 0.25;
                float ttl = (float)Utilities.UtilRandom.NextDouble() * 5f + 2f;
                double speed = Utilities.UtilRandom.NextDouble();
                Location loc = new Location(ssize);
                Engine.AddEffect(ParticleEffectType.SQUARE, (o) => pos + (forward * size * speed) * o.TTL / o.O_TTL, (o) => loc, (o) => 0, ttl, c1, c2, true, tex);
            }
        }

        public void Smoke(Location pos, float spread, Location color)
        {
            double xoff = Utilities.UtilRandom.NextDouble() * spread - spread * 0.5;
            double yoff = Utilities.UtilRandom.NextDouble() * spread - spread * 0.5;
            double zoff = Utilities.UtilRandom.NextDouble() * spread - spread * 0.5;
            Engine.AddEffect(ParticleEffectType.SQUARE, (o) => pos + new Location(xoff, yoff, -TheClient.TheRegion.PhysicsWorld.ForceUpdater.Gravity.Z * 0.33f + zoff) * (1 - o.TTL / o.O_TTL)
                + new Location(xoff, yoff, 0) * Math.Sqrt(1 - o.TTL / o.O_TTL), // TODO: Speed this up a lotta bit!
                (o) => new Location(1f), (o) => 0, 10, color, color, true, SmokeT);
        }

        public void BigSmoke(Location pos, float spread, Location color)
        {
            double xoff = Utilities.UtilRandom.NextDouble() * spread - spread * 0.5;
            double yoff = Utilities.UtilRandom.NextDouble() * spread - spread * 0.5;
            double zoff = Utilities.UtilRandom.NextDouble() * spread - spread * 0.5;
            Engine.AddEffect(ParticleEffectType.SQUARE, (o) => pos + new Location(xoff, yoff, -TheClient.TheRegion.PhysicsWorld.ForceUpdater.Gravity.Z * 0.5f + zoff) * (1 - o.TTL / o.O_TTL),
                (o) => new Location(10f * (1 - o.TTL / o.O_TTL)), (o) => 0, 35, color, color, true, SmokeT);
        }

        public void PathMark(Location pos, Func<Location> target)
        {
            Location height = new Location(0, 0, 5);
            Location height2 = new Location(0, 0, 4);
            Location height3 = new Location(0, 0, 3);
            Engine.AddEffect(ParticleEffectType.CYLINDER, (o) => pos, (o) => pos + height * ((o.TTL / o.O_TTL) / 2 + 0.5f),
                (o) => 0.5f, 4f, new Location(0, 1, 1), new Location(0, 1, 1), true, White_Blur, 0.70f);
            Engine.AddEffect(ParticleEffectType.CYLINDER, (o) => pos, (o) => pos + height * ((o.TTL / o.O_TTL) / 2 + 0.5f),
                (o) => 0.6f, 4f, new Location(0, 1, 1), new Location(0, 1, 1), true, White_Blur, 0.55f);
            Engine.AddEffect(ParticleEffectType.CYLINDER, (o) => pos, (o) => pos + height * ((o.TTL / o.O_TTL) / 2 + 0.5f),
                (o) => 0.7f, 4f, new Location(0, 1, 1), new Location(0, 1, 1), true, White_Blur, 0.40f);
            Engine.AddEffect(ParticleEffectType.LINE, (o) => pos, (o) => target(), (o) => 1, 4f, new Location(0, 0.5f, 0.5f), new Location(0, 0.5f, 0.5f), true, White, 1);
        }
    }
}
