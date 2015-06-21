using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class DespawnEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8)
            {
                return false;
            }
            Entity e = TheClient.GetEntity(Utilities.BytesToLong(data));
            if (e == null)
            {
                return false;
            }
            TheClient.Despawn(e);
            return true;
        }
    }
}
