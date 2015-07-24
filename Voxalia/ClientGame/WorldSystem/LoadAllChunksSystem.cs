using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Voxalia.ClientGame.WorldSystem
{
    public class LoadAllChunksSystem
    {
        public World TheWorld;

        public LoadAllChunksSystem(World tworld)
        {
            TheWorld = tworld;
        }

        public int Count = 0;

        int c = 0;

        int rC = 0;

        public Object Locker = new Object();


        public void LoadAll()
        {
            foreach (Chunk chunk in TheWorld.LoadedChunks.Values)
            {
                Count++;
                chunk.AddToWorld(() =>
                {
                    lock (Locker)
                    {
                        c++;
                    }
                });
                chunk.CreateVBO(() =>
                {
                    lock (Locker)
                    {
                        rC++;
                    }
                });
            }
            while (Count < 0)
            {
                bool waitmore;
                lock (Locker)
                {
                    waitmore = Count > c || Count > rC;
                }
                if (!waitmore)
                {
                    break;
                }
                TheWorld.TheClient.Schedule.RunAllSyncTasks(0.016); // TODO: Separate World scheduler
                // TODO: Un-freeze the client window?!
                Thread.Sleep(16);
            }
        }
    }
}
