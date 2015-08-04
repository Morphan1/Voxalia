using System.Collections.Generic;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;
using OpenTK.Graphics;
using System.Linq;

namespace Voxalia.ClientGame.GraphicsSystems.ParticleSystem
{
    public class ParticleEngine
    {
        public Client TheClient;

        public ParticleEngine(Client tclient)
        {
            TheClient = tclient;
            ActiveEffects = new List<ParticleEffect>();
        }

        public List<ParticleEffect> ActiveEffects;

        public void AddEffect(ParticleEffectType type, Location one, Location two, Location end, float data, float ttl, Location color, Location color2, bool fades, Texture texture, float salpha = 1)
        {
            ActiveEffects.Add(new ParticleEffect(TheClient) { Type = type, One = one, Two = two, EndPos = end, Data = data, TTL = ttl, O_TTL = ttl, Color = color, Color2 = color2, Alpha = salpha, Fades = fades, texture = texture });
        }

        public void Render()
        {
            TheClient.Rendering.SetMinimumLight(1);
            ActiveEffects = ActiveEffects.OrderBy(o => -(o.One - TheClient.CameraPos).LengthSquared()).ToList();
            for (int i = 0; i < ActiveEffects.Count; i++)
            {
                ActiveEffects[i].Render();
                if (ActiveEffects[i].TTL <= 0)
                {
                    ActiveEffects.RemoveAt(i--);
                }
            }
            TheClient.Textures.White.Bind();
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.SetMinimumLight(0);
        }
    }
}
