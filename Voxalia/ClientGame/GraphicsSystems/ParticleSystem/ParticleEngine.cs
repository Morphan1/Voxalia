﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;

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

        public void AddEffect(ParticleEffectType type, Location one, Location two, float data, float ttl, Location color, bool fades, Texture texture)
        {
            ActiveEffects.Add(new ParticleEffect(TheClient) { Type = type, One = one, Two = two, Data = data, TTL = ttl, O_TTL = ttl, Color = color, Alpha = 1, Fades = fades, texture = texture });
        }

        public void Render()
        {
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
        }
    }
}