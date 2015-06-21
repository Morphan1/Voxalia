﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class PingPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 1)
            {
                return false;
            }
            byte bit = data[0];
            if (Chunk)
            {
                TheClient.Network.SendChunkPacket(new PingPacketOut(bit));
            }
            else
            {
                TheClient.Network.SendPacket(new PingPacketOut(bit));
            }
            return true;
        }
    }
}
