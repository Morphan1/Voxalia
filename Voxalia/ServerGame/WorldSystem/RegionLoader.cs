using System;
using Voxalia.Shared;
using System.Threading;

namespace Voxalia.ServerGame.WorldSystem
{
    public class RegionLoader
    {
        public int Count = 0;

        int c = 0;

        public Object Locker = new Object();

        public Region region;

        public void LoadRegion(Location min, Location max)
        {
            Location minc = region.ChunkLocFor(min);
            Location maxc = region.ChunkLocFor(max);
            for (double x = minc.X; x <= maxc.X; x++)
            {
                for (double y = minc.Y; y <= maxc.Y; y++)
                {
                    for (double z = minc.Z; z <= maxc.Z; z++)
                    {
                        lock (Locker)
                        {
                            c++;
                        }
                        region.LoadChunk_Background(new Location(x, y, z), (o) =>
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
