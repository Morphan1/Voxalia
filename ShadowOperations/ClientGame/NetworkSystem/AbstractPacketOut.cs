using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.ClientGame.NetworkSystem
{
    public abstract class AbstractPacketOut
    {
        /// <summary>
        /// The ID of this packet.
        /// </summary>
        public byte ID = 0;

        /// <summary>
        /// The binary data held in this packet.
        /// </summary>
        public byte[] Data;
    }
}
