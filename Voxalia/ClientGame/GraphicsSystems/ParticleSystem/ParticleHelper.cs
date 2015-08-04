using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.GraphicsSystems.ParticleSystem
{
    public class ParticleHelper
    {
        Texture[] Explosion;

        public ParticleHelper(TextureEngine engine)
        {
            Explosion = new Texture[3];
            for (int i = 0; i < 3; i++)
            {
                Explosion[i] = engine.GetTexture("effects/explosion/0" + (i + 1));
            }
        }

        public void Sort()
        {
            Engine.ActiveEffects = Engine.ActiveEffects.OrderBy(o => (Engine.TheClient.CameraPos - o.GetCPos()).LengthSquared()).ToList();
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
                Engine.AddEffect(ParticleEffectType.SQUARE, pos, new Location(ssize), pos + forward * size * speed, 0, ttl, c1, c2, true, tex);
            }
        }
    }
}
