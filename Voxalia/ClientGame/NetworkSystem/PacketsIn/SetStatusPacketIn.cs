using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SetStatusPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 1 + 1)
            {
                return false;
            }
            long eid = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            Entity e = TheClient.TheRegion.GetEntity(eid);
            if (!(e is CharacterEntity))
            {
                return false;
            }
            switch ((ClientStatus)data[8])
            {
                case ClientStatus.TYPING:
                    ((CharacterEntity)e).IsTyping = (data[8 + 1] != 0);
                    return true;
                default:
                    return false;
            }
        }
    }
}
