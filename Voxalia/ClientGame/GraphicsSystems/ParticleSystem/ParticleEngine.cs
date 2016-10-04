using System;
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

        public ParticleEffect AddEffect(ParticleEffectType type, Func<ParticleEffect, Location> start, Func<ParticleEffect, Location> end,
            Func<ParticleEffect, float> fdata, float ttl, Location color, Location color2, bool fades, Texture texture, float salpha = 1)
        {
            ParticleEffect pe = new ParticleEffect(TheClient) { Type = type, Start = start, End = end, FData = fdata, TTL = ttl, O_TTL = ttl, Color = color, Color2 = color2, Alpha = salpha, Fades = fades, texture = texture };
            ActiveEffects.Add(pe);
            return pe;
        }

        public void Render()
        {
            TheClient.Rendering.SetMinimumLight(1);
            for (int i = 0; i < ActiveEffects.Count; i++)
            {
                ActiveEffects[i].Render();
                if (ActiveEffects[i].TTL <= 0)
                {
                    ActiveEffects[i].OnDestroy?.Invoke(ActiveEffects[i]);
                    ActiveEffects.RemoveAt(i--);
                }
            }
            TheClient.Textures.White.Bind();
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.SetMinimumLight(0);
        }
    }
}
