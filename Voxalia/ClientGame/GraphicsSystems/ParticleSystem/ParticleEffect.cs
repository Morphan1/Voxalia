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

        public Func<ParticleEffect, float> AltAlpha = null;

        public Location Color;

        public Location Color2;
        
        public Location MinLight = new Location(0, 0, 0);

        public Texture texture;

        public Action OnDestroy = null;

        public bool BlowsInWind = true;

        public Location WindOffset = Location.Zero;

        public static float FadeInOut(ParticleEffect pe)
        {
            float rel = pe.TTL / pe.O_TTL;
            if (rel >= 0.5)
            {
                return 1 - ((rel - 0.5f) * 2);
            }
            else
            {
                return rel * 2;
            }
        }

        public ParticleEffect(Client tclient)
        {
            TheClient = tclient;
        }
        
        public void Render()
        {
            TTL -= (float)TheClient.gDelta;
            if (TTL <= 0)
            {
                return;
            }
            if (AltAlpha != null)
            {
                Alpha = AltAlpha(this);
                if (Alpha <= 0.01)
                {
                    return;
                }
            }
            else if (Fades)
            {
                Alpha -= (float)TheClient.gDelta / O_TTL;
                if (Alpha <= 0.01)
                {
                    TTL = 0;
                    return;
                }
            }
            float rel = TTL / O_TTL;
            if (rel >= 1 || rel <= 0)
            {
                return;
            }
            texture.Bind();
            Location start = Start(this) + WindOffset;
            if (BlowsInWind)
            {
                WindOffset += TheClient.TheRegion.ActualWind * SimplexNoiseInternal.Generate((start.X + TheClient.GlobalTickTimeLocal) * 0.2, (start.Y + TheClient.GlobalTickTimeLocal) * 0.2, start.Z * 0.2) * 0.1;
            }
            Location ligl = TheClient.TheRegion.GetLightAmount(start, Location.UnitZ, null);
            Vector4 light = new Vector4((float)ligl.X, (float)ligl.Y, (float)ligl.Z, 1.0f);
            light.X = (float)Math.Max(light.X, MinLight.X);
            light.Y = (float)Math.Max(light.Y, MinLight.Y);
            light.Z = (float)Math.Max(light.Z, MinLight.Z);
            Vector4 scolor = new Vector4((float)Color.X * light.X, (float)Color.Y * light.Y, (float)Color.Z * light.Z, Alpha * light.W);
            Vector4 scolor2 = new Vector4((float)Color2.X * light.X, (float)Color2.Y * light.Y, (float)Color2.Z * light.Z, Alpha * light.W);
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
                        Matrix4d mat = Matrix4d.Scale(ClientUtilities.ConvertD(End(this))) * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(start));
                        TheClient.MainWorldView.SetMatrix(2, mat);
                        TheClient.Models.Cube.Draw();
                    }
                    break;
                case ParticleEffectType.SPHERE:
                    {
                        Matrix4d mat = Matrix4d.Scale(ClientUtilities.ConvertD(End(this))) * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(start));
                        TheClient.MainWorldView.SetMatrix(2, mat);
                        TheClient.Models.Sphere.Draw();
                    }
                    break;
                case ParticleEffectType.SQUARE:
                    {
                        TheClient.Rendering.RenderBillboard(start, End(this), TheClient.MainWorldView.CameraPos);
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
