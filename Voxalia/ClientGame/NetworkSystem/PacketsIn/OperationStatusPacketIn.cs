
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using System.Threading;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class OperationStatusPacketIn: AbstractPacketIn
    {
        public int ChunksStillLoading()
        {
            int c = 0;
            foreach (Chunk chunk in TheClient.TheRegion.LoadedChunks.Values)
            {
                if (chunk.LOADING)
                {
                    c++;
                }
            }
            return c;
        }

        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 2)
            {
                return false;
            }
            switch ((StatusOperation)data[0])
            {
                case StatusOperation.CHUNK_LOAD:
                    if (data[1] == 1)
                    {
                        TheClient.Schedule.StartASyncTask(() =>
                        {
                            int c = 1;
                            while (c > 0)
                            {
                                TheClient.Schedule.ScheduleSyncTask(() =>
                                {
                                    TheClient.TheChunkWaitingScreen.ChunksStillWaiting = ChunksStillLoading();
                                });
                                Thread.Sleep(16);
                                c = TheClient.TheChunkWaitingScreen.ChunksStillWaiting;
                            }
                            TheClient.ShowGame();
                        });
                    }
                    else if (data[1] == 0)
                    {
                        TheClient.ShowChunkWaiting();
                    }
                    else if (data[1] == 2)
                    {
                        TheClient.ProcessChunks();
                    }
                    else
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
