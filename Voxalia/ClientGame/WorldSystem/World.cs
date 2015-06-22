using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.WorldSystem
{
    public class World
    {
        public Dictionary<Location, Chunk> LoadedChunks = new Dictionary<Location, Chunk>();

        public Client TheClient;

        public Chunk GetChunk(Location pos)
        {
            pos.X = (int)pos.X - (int)pos.X % 30;
            pos.Y = (int)pos.Y - (int)pos.Y % 30;
            pos.Z = (int)pos.Z - (int)pos.Z % 30;
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                return chunk;
            }
            chunk = new Chunk();
            chunk.OwningWorld = this;
            chunk.WorldPosition = pos;
            LoadedChunks.Add(pos, chunk);
            return chunk;
        }

        public void Render()
        {
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.SetMinimumLight(0f);
            Matrix4 mat = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref mat);
            GL.UniformMatrix4(7, false, ref mat);
            foreach (Chunk chunk in LoadedChunks.Values)
            {
                // TODO: If chunk.InFrustum(frustum) { }
                chunk.Render();
            }
        }
    }
}
