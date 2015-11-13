using System;
using System.Threading;
using System.Collections.Generic;
using Voxalia.Shared;

namespace Voxalia.ClientGame.WorldSystem
{
    public class LoadAllChunksSystem
    {
        public Region TheRegion;

        public LoadAllChunksSystem(Region tregion)
        {
            TheRegion = tregion;
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
            TheRegion.TheClient.Schedule.StartASyncTask(() =>
            {
                // The following code is the _correct_ way to do this, more or less...
                // (This was originally just a foreach, now it's broken into components to demonstrate.)
                // However... it freezes. Inside the MoveNext(). Good job, Microsoft!
                // Dictionary<Location, Chunk>.ValueCollection.Enumerator x = TheRegion.LoadedChunks.Values.GetEnumerator();
                // while (x.MoveNext())
                // {
                // ...
                // Now, observe the greatest work-around of all time!
                Chunk[] chunks = new Chunk[TheRegion.LoadedChunks.Values.Count];
                TheRegion.LoadedChunks.Values.CopyTo(chunks, 0);
                foreach (Chunk chunk in chunks)
                {
                    if (!chunk.PROCESSED)
                    {
                        Count++;
                        chunk.CalculateLighting();
                        TheRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                        {
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
                        });
                        chunk.PROCESSED = true;
                    }
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
