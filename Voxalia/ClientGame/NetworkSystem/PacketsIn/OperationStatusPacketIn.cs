
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
                default:
                    return false;
            }
        }
    }
}
