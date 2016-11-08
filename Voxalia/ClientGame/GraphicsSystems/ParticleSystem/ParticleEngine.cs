//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared;
using OpenTK.Graphics;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.GraphicsSystems.ParticleSystem
{
    public class ParticleEngine
    {
        public Client TheClient;

        public int Part_VAO = -1;
        public int Part_VBO_Pos = -1;
        public int Part_VBO_Ind = -1;
        public int Part_VBO_Col = -1;
        public int Part_VBO_Tcs = -1;
        public int Part_C;

        public ParticleEngine(Client tclient)
        {
            TheClient = tclient;
            ActiveEffects = new List<ParticleEffect>();
            Part_VAO = GL.GenVertexArray();
            Part_VBO_Pos = GL.GenBuffer();
            Part_VBO_Ind = GL.GenBuffer();
            Part_VBO_Col = GL.GenBuffer();
            Part_VBO_Tcs = GL.GenBuffer();
            Part_C = 0;
        }

        public List<ParticleEffect> ActiveEffects;

        public ParticleEffect AddEffect(ParticleEffectType type, Func<ParticleEffect, Location> start, Func<ParticleEffect, Location> end,
            Func<ParticleEffect, float> fdata, float ttl, Location color, Location color2, bool fades, Texture texture, float salpha = 1)
        {
            ParticleEffect pe = new ParticleEffect(TheClient) { Type = type, Start = start, End = end, FData = fdata, TTL = ttl, O_TTL = ttl, Color = color, Color2 = color2, Alpha = salpha, Fades = fades, texture = texture };
            ActiveEffects.Add(pe);
            return pe;
        }

        bool prepped = false;

        public void Render()
        {
            if (TheClient.MainWorldView.FBOid == FBOID.FORWARD_TRANSP)
            {
                List<Vector3> pos = new List<Vector3>();
                List<Vector4> col = new List<Vector4>();
                List<Vector2> tcs = new List<Vector2>(0);
                for (int i = 0; i < ActiveEffects.Count; i++)
                {
                    if (ActiveEffects[i].Type == ParticleEffectType.SQUARE)
                    {
                        Tuple<Location, Vector4, Vector2> dets = ActiveEffects[i].GetDetails();
                        if (dets != null)
                        {
                            pos.Add(ClientUtilities.Convert(dets.Item1 - TheClient.MainWorldView.CameraPos));
                            col.Add(Vector4.Min(dets.Item2, Vector4.One)); // NOTE: Min here is only for FORWARD mode, to prevent light > 1.0
                            tcs.Add(dets.Item3);
                        }
                    }
                    else
                    {
                        ActiveEffects[i].Render(); // TODO: Deprecate / remove / fully replace!?
                    }
                    if (ActiveEffects[i].TTL <= 0)
                    {
                        ActiveEffects[i].OnDestroy?.Invoke(ActiveEffects[i]);
                        ActiveEffects.RemoveAt(i--);
                    }
                }
                TheClient.s_forw_particles = TheClient.s_forw_particles.Bind();
                GL.UniformMatrix4(1, false, ref TheClient.MainWorldView.PrimaryMatrix);
                Matrix4 ident = Matrix4.Identity;
                GL.UniformMatrix4(2, false, ref ident);
                TheClient.Textures.GetTexture("effects/fire/whiteflamelick01").Bind(); // TODO: Texture2DArray!
                Vector3[] posset = pos.ToArray();
                Vector4[] colorset = col.ToArray();
                Vector2[] texcoords = tcs.ToArray();
                uint[] posind = new uint[posset.Length];
                for (uint i = 0; i < posind.Length; i++)
                {
                    posind[i] = i;
                }
                Part_C = posind.Length;
                GL.BindBuffer(BufferTarget.ArrayBuffer, Part_VBO_Pos);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(posset.Length * OpenTK.Vector3.SizeInBytes), posset, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Part_VBO_Tcs);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texcoords.Length * OpenTK.Vector2.SizeInBytes), texcoords, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Part_VBO_Col);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colorset.Length * OpenTK.Vector4.SizeInBytes), colorset, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, Part_VBO_Ind);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(posind.Length * sizeof(uint)), posind, BufferUsageHint.StaticDraw);
                GL.BindVertexArray(Part_VAO);
                if (!prepped)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Part_VBO_Pos);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Part_VBO_Tcs);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(2);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Part_VBO_Col);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(4);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, Part_VBO_Ind);
                    prepped = true;
                }
                GL.DrawElements(PrimitiveType.Points, Part_C, DrawElementsType.UnsignedInt, IntPtr.Zero);
                GL.BindVertexArray(0);
                TheClient.isVox = true;
                TheClient.SetEnts();
            }
            else if (TheClient.MainWorldView.FBOid.IsMainTransp())
            {
                // TODO: Translate this to use the above, with a new shader.
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
}
