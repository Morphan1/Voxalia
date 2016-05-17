using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem
{
    public abstract class AbstractPacketOut
    {
        /// <summary>
        /// The ID of this packet.
        /// </summary>
        public ServerToClientPacket ID = 0;

        /// <summary>
        /// The binary data held in this packet.
        /// </summary>
        public byte[] Data;
    }
}
