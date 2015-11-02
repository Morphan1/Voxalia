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
            Vector4 scolor = new Vector4((float)Color.X, (float)Color.Y, (float)Color.Z, Alpha);
            Vector4 scolor2 = new Vector4((float)Color2.X, (float)Color2.Y, (float)Color2.Z, Alpha);
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
                        TheClient.Rendering.RenderLine(Start(this), End(this));
                        if (dat != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.CYLINDER:
                    {
                        TheClient.Rendering.RenderCylinder(Start(this), End(this), FData(this));
                    }
                    break;
                case ParticleEffectType.LINEBOX:
                    {
                        float dat = FData(this);
                        if (dat != 1)
                        {
                            GL.LineWidth(dat);
                        }
                        TheClient.Rendering.RenderLineBox(Start(this), End(this));
                        if (dat != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.BOX:
                    {
                        Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(End(this))) * Matrix4.CreateTranslation(ClientUtilities.Convert(Start(this)));
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Cube.Draw();
                    }
                    break;
                case ParticleEffectType.SPHERE:
                    {
                        Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(End(this))) * Matrix4.CreateTranslation(ClientUtilities.Convert(Start(this)));
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Sphere.Draw();
                    }
                    break;
                case ParticleEffectType.SQUARE:
                    {
                        TheClient.Rendering.RenderBillboard(Start(this), End(this), TheClient.CameraPos);
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
