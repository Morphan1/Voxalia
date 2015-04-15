using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class YourPositionPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            TheClient.Player.SetPosition(pos); // TODO: better prediction
            return true;
        }
    }
}
