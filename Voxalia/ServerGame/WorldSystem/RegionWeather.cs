using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        public Location Wind = new Location(0.3f, 0, 0);

        public void TickClouds()
        {
            foreach (Chunk chunk in LoadedChunks.Values)
            {
                // TODO: Only if pure air?
                if (chunk.WorldPosition.Z >= 3 && chunk.WorldPosition.Z <= 7) // TODO: Better estimating
                {
                    if (Utilities.UtilRandom.Next(400) > 399)
                    {
                        double d1 = Utilities.UtilRandom.NextDouble() * Chunk.CHUNK_SIZE;
                        double d2 = Utilities.UtilRandom.NextDouble() * Chunk.CHUNK_SIZE;
                        double d3 = Utilities.UtilRandom.NextDouble() * Chunk.CHUNK_SIZE;
                        Cloud cloud = new Cloud(this, chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(d1, d2, d3));
                        SpawnCloud(cloud);
                    }
                }
            }
            for (int i = Clouds.Count - 1; i >= 0; i--)
            {
                // TODO: if in non-air chunk, dissipate rapidly?
                Location ppos = Clouds[i].Position;
                Clouds[i].Position = ppos + Wind + Clouds[i].Velocity;
                bool changed = (Utilities.UtilRandom.Next(100) > Clouds[i].Points.Count)
                    && (Utilities.UtilRandom.Next(100) > Clouds[i].Points.Count)
                    && (Utilities.UtilRandom.Next(100) > Clouds[i].Points.Count)
                    && (Utilities.UtilRandom.Next(100) > Clouds[i].Points.Count);
                for (int s = 0; s < Clouds[i].Sizes.Count; s++)
                {
                    Clouds[i].Sizes[s] += 0.05f;
                    if (Clouds[i].Sizes[s] > Clouds[i].EndSizes[s])
                    {
                        Clouds[i].Sizes[s] = Clouds[i].EndSizes[s];
                    }
                }
                foreach (PlayerEntity player in Players)
                {
                    bool prev = player.ShouldSeeLODPositionOneSecondAgo(ppos);
                    bool curr = player.ShouldLoadPosition(Clouds[i].Position);
                    if (prev && !curr)
                    {
                        player.Network.SendPacket(new RemoveCloudPacketOut(Clouds[i].CID));
                    }
                    else if (curr && (Clouds[i].IsNew || !prev))
                    {
                        player.Network.SendPacket(new AddCloudPacketOut(Clouds[i]));
                    }
                }
                Clouds[i].IsNew = false;
                if (changed)
                {
                    AddToCloud(Clouds[i], 0f);
                    foreach (PlayerEntity player in Players)
                    {
                        bool curr = player.ShouldLoadPosition(Clouds[i].Position);
                        if (curr)
                        {
                            player.Network.SendPacket(new AddToCloudPacketOut(Clouds[i], Clouds[i].Points.Count - 1));
                        }
                    }
                }
                Vector3i cpos = ChunkLocFor(Clouds[i].Position);
                if (!LoadedChunks.ContainsKey(cpos))
                {
                    DeleteCloud(Clouds[i]);
                    continue;
                }
            }
            foreach (PlayerEntity player in Players)
            {
                player.losPos = player.GetPosition();
            }
        }

        public void AddToCloud(Cloud cloud, float start)
        {
            double modif = Math.Sqrt(cloud.Points.Count) * 1.5;
            double d1 = Utilities.UtilRandom.NextDouble() * modif * 2 - modif;
            double d2 = Utilities.UtilRandom.NextDouble() * modif * 2 - modif;
            double d3 = Utilities.UtilRandom.NextDouble() * modif * 2 - modif;
            double d4 = Utilities.UtilRandom.NextDouble() * 10 * modif;
            cloud.Points.Add(new Location(d1, d2, d3));
            cloud.Sizes.Add(start > d4 ? (float)d4 : start);
            cloud.EndSizes.Add((float)d4);
        }

        public void DeleteCloud(Cloud cloud)
        {
            foreach (PlayerEntity player in Players)
            {
                if (player.ShouldSeePosition(cloud.Position))
                {
                    player.Network.SendPacket(new RemoveCloudPacketOut(cloud.CID));
                }
            }
            Clouds.Remove(cloud);
        }

        public void RemoveCloudsFrom(Chunk chunk)
        {
            for (int i = Clouds.Count - 1; i >= 0; i--)
            {
                if (chunk.Contains(Clouds[i].Position))
                {
                    DeleteCloud(Clouds[i]);
                }
            }
        }

        public void AddCloudsToNewChunk(Chunk chunk)
        {
            if (chunk.WorldPosition.Z >= 3 && chunk.WorldPosition.Z <= 7 && Utilities.UtilRandom.Next(100) > 90)
            {
                double d1 = Utilities.UtilRandom.NextDouble() * Chunk.CHUNK_SIZE;
                double d2 = Utilities.UtilRandom.NextDouble() * Chunk.CHUNK_SIZE;
                double d3 = Utilities.UtilRandom.NextDouble() * Chunk.CHUNK_SIZE;
                Cloud cloud = new Cloud(this, chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(d1, d2, d3));
                int rand = Utilities.UtilRandom.Next(7) > 2 ? Utilities.UtilRandom.Next(50) + 50: Utilities.UtilRandom.Next(100);
                for (int i = 0; i < rand; i++)
                {
                    AddToCloud(cloud, 10f);
                }
                SpawnCloud(cloud);
            }
        }

        public void SpawnCloud(Cloud cloud)
        {
            cloud.CID = TheServer.AdvanceCloudID();
            Clouds.Add(cloud);
        }

        public List<Cloud> Clouds = new List<Cloud>();
    }
}
