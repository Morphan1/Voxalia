using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class OperationStatusPacketIn: AbstractPacketIn
    {
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
                        TheClient.ShowGame();
                    }
                    else if (data[1] == 0)
                    {
                        TheClient.ShowChunkWaiting();
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
