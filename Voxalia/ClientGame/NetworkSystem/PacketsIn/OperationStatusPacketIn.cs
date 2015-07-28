using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using System.Threading;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class OperationStatusPacketIn: AbstractPacketIn
    {
        public bool ChunksStillLoading()
        {
            foreach (Chunk chunk in TheClient.TheWorld.LoadedChunks.Values)
            {
                if (chunk.LOADING)
                {
                    return true;
                }
            }
            return false;
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
                            while (ChunksStillLoading())
                            {
                                Thread.Sleep(16);
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
                    SysConsole.Output(OutputType.INFO, "Unknown status operation: " + ((StatusOperation)data[0]));
                    return false;
            }
            return true;
        }
    }
}
