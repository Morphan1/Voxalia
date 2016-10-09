//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ClientGame.NetworkSystem.PacketsOut;

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
            if (ChunkN)
            {
                TheClient.Network.SendChunkPacket(new PingPacketOut(bit));
            }
            else
            {
                TheClient.Network.SendPacket(new PingPacketOut(bit));
                TheClient.LastPingValue = TheClient.GlobalTickTimeLocal - TheClient.LastPingTime;
                TheClient.LastPingTime = TheClient.GlobalTickTimeLocal;
            }
            return true;
        }
    }
}
