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

        public Location One;

        public Location Two;

        public float Data;

        public float Alpha = 1;

        public float TTL;

        public float O_TTL;

        public bool Fades;

        public Location Color;

        public Location Color2;

        public Location EndPos;

        public Texture texture;

        public ParticleEffect(Client tclient)
        {
            TheClient = tclient;
        }

        public Location GetCPos()
        {
            float rel = TTL / O_TTL;
            return (EndPos - One) * (1 - rel);
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
            Location cpos = (EndPos - One) * (1 - rel);
            switch (Type)
            {
                case ParticleEffectType.LINE:
                    {
                        if (Data != 1)
                        {
                            GL.LineWidth(Data);
                        }
                        TheClient.Rendering.RenderLine(cpos + One, cpos + Two);
                        if (Data != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.CYLINDER:
                    {
                        TheClient.Rendering.RenderCylinder(cpos + One, cpos + Two, Data);
                    }
                    break;
                case ParticleEffectType.LINEBOX:
                    {
                        if (Data != 1)
                        {
                            GL.LineWidth(Data);
                        }
                        TheClient.Rendering.RenderLineBox(cpos + One, cpos + Two);
                        if (Data != 1)
                        {
                            GL.LineWidth(1);
                        }
                    }
                    break;
                case ParticleEffectType.BOX:
                    {
                        Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(Two)) * Matrix4.CreateTranslation(ClientUtilities.Convert(cpos + One));
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Cube.Draw();
                    }
                    break;
                case ParticleEffectType.SPHERE:
                    {
                        Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(Two)) * Matrix4.CreateTranslation(ClientUtilities.Convert(cpos + One));
                        GL.UniformMatrix4(2, false, ref mat);
                        TheClient.Models.Sphere.Draw();
                    }
                    break;
                case ParticleEffectType.SQUARE:
                    {
                        TheClient.Rendering.RenderBillboard(cpos + One, Two, TheClient.CameraPos);
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
