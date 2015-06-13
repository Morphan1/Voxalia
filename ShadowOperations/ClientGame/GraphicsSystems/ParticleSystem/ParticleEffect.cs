using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace ShadowOperations.ClientGame.GraphicsSystems.ParticleSystem
{
    public class ParticleEffect
    {
        public Client TheClient;

        public ParticleEffectType Type;

        public Location One;

        public Location Two;

        public float Data;

        public float Alpha = 1;

        public float TTL;

        public float O_TTL;

        public bool Fades;

        public Location Color;

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
            }
            texture.Bind();
            TheClient.Rendering.SetColor(new Color4((byte)(255 * Color.X), (byte)(255 * Color.Y), (byte)(255 * Color.Z), (byte)(255 * Alpha)));
            switch (Type)
            {
                case ParticleEffectType.LINE:
                    {
                        if (Data != 1)
                        {
                            GL.LineWidth(Data);
                        }
                        TheClient.Rendering.RenderLine(One, Two);
                        if (Data != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.CYLINDER:
                    {
                        TheClient.Rendering.RenderCylinder(One, Two, Data);
                    }
                    break;
                case ParticleEffectType.LINEBOX:
                    {
                        if (Data != 1)
                        {
                            GL.LineWidth(Data);
                        }
                        TheClient.Rendering.RenderLineBox(One, Two);
                        if (Data != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.BOX:
                    {
                        Matrix4 mat = Matrix4.CreateScale(Two.ToOVector()) * Matrix4.CreateTranslation(One.ToOVector());
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Cube.Draw();
                    }
                    break;
                case ParticleEffectType.SPHERE:
                    {
                        Matrix4 mat = Matrix4.CreateScale(Two.ToOVector()) * Matrix4.CreateTranslation(One.ToOVector());
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Sphere.Draw();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public ParticleEffect Clone()
        {
            return new ParticleEffect(TheClient) { Type = Type, One = One, Two = Two, Data = Data };
        }
    }
}
