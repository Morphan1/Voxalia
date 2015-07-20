using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using System.Threading;

namespace Voxalia.ServerGame.WorldSystem
{
    public class RegionLoader
    {
        public int Count = 0;

        int c = 0;

        public Object Locker = new Object();

        public World world;

        public void LoadRegion(Location min, Location max)
        {
            Location minc = world.ChunkLocFor(min);
            Location maxc = world.ChunkLocFor(max);
            for (double x = minc.X; x <= maxc.X; x++)
            {
                for (double y = minc.Y; y <= maxc.Y; y++)
                {
                    for (double z = minc.Z; z <= maxc.Z; z++)
                    {
                        c++;
                        world.LoadChunk_Background(new Location(x, y, z), (o) =>
                        {
                            lock (Locker)
                            {
                                if (o)
                                {
                                    c--;
                                }
                                else
                                {
                                    Count++;
                                }
                            }
                        });
                    }
                }
            }
            while (true)
            {
                bool waitmore;
                lock (Locker)
                {
                    waitmore = c > Count;
                }
                if (!waitmore)
                {
                    break;
                }
                Thread.Sleep(16);
            }
        }
    }
}
