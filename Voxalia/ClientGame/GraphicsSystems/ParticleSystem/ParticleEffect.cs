using System;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems.ParticleSystem
{
    public class ParticleEffect
    {
        public Client TheClient;

        public ParticleEffectType Type;

        public Func<ParticleEffect, Location> Start;

        public Func<ParticleEffect, Location> End;

        public Func<ParticleEffect, float> FData;
        
        public float Alpha = 1;

        public float TTL;

        public float O_TTL;

        public bool Fades;

        public Location Color;

        public Location Color2;

        public Location MinLight = new Location(0, 0, 0);

        public Texture texture;

        public ParticleEffect(Client tclient)
        {
            TheClient = tclient;
        }
        
        public void Render()
        {
            TTL -= (float)TheClient.gDelta;
            if (Fades)
            {
                Alpha -= (float)TheClient.gDelta / O_TTL;
                if (Alpha <= 0)
                {
                    TTL = 0;
                    return;
                }
            }
            texture.Bind();
            Location start = Start(this);
            Vector4 light = TheClient.TheRegion.GetBlockLighting(start);
            light.X = (float)Math.Max(light.X, MinLight.X);
            light.Y = (float)Math.Max(light.Y, MinLight.Y);
            light.Z = (float)Math.Max(light.Z, MinLight.Z);
            Vector4 scolor = new Vector4((float)Color.X * light.X, (float)Color.Y * light.Y, (float)Color.Z * light.Z, Alpha * light.W);
            Vector4 scolor2 = new Vector4((float)Color2.X * light.X, (float)Color2.Y * light.Y, (float)Color2.Z * light.Z, Alpha * light.W);
            float rel = TTL / O_TTL;
            TheClient.Rendering.SetColor(scolor * rel + scolor2 * (1 - rel));
            switch (Type)
            {
                case ParticleEffectType.LINE:
                    {
                        float dat = FData(this);
                        if (dat != 1)
                        {
                            GL.LineWidth(dat);
                        }
                        TheClient.Rendering.RenderLine(start, End(this));
                        if (dat != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.CYLINDER:
                    {
                        TheClient.Rendering.RenderCylinder(start, End(this), FData(this));
                    }
                    break;
                case ParticleEffectType.LINEBOX:
                    {
                        float dat = FData(this);
                        if (dat != 1)
                        {
                            GL.LineWidth(dat);
                        }
                        TheClient.Rendering.RenderLineBox(start, End(this));
                        if (dat != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.BOX:
                    {
                        Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(End(this))) * Matrix4.CreateTranslation(ClientUtilities.Convert(start));
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Cube.Draw();
                    }
                    break;
                case ParticleEffectType.SPHERE:
                    {
                        Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(End(this))) * Matrix4.CreateTranslation(ClientUtilities.Convert(start));
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Sphere.Draw();
                    }
                    break;
                case ParticleEffectType.SQUARE:
                    {
                        TheClient.Rendering.RenderBillboard(start, End(this), TheClient.CameraPos);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public ParticleEffect Clone()
        {
            return (ParticleEffect) MemberwiseClone();
        }
    }
}
