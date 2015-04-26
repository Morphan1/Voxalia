using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnBulletPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 4 + 12 + 12)
            {
                return false;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(TheClient, false);
            bpe.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            bpe.scale = new Location(Utilities.BytesToFloat(Utilities.BytesPartial(data, 8, 4)));
            bpe.SetPosition(Location.FromBytes(data, 8 + 4));
            bpe.SetVelocity(Location.FromBytes(data, 8 + 4 + 12));
            TheClient.SpawnEntity(bpe);
            return true;
        }
    }
}
