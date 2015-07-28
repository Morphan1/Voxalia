using System;
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

        public int c = 0;

        public int rC = 0;

        public Object Locker = new Object();


        public void LoadAll(Action callback)
        {
            Count = 0;
            c = 0;
            rC = 0;
            TheWorld.TheClient.Schedule.StartASyncTask(() =>
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
                    chunk.PROCESSED = true;
                }
                while (true)
                {
                    lock (Locker)
                    {
                        if (c >= Count && rC >= Count)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(16);
                }
                callback.Invoke();
            });
        }
    }
}
