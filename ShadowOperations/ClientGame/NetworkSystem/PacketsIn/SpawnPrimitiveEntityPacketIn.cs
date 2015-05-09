using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class SpawnPrimitiveEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 12 + 12 + 12 + 8)
            {
                return false;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(TheClient, false);
            bpe.model = TheClient.Models.GetModel("projectiles/arrow.dae");
            bpe.Position = Location.FromBytes(data, 0);
            bpe.Velocity = Location.FromBytes(data, 12);
            bpe.Angle = Location.FromBytes(data, 12 + 12);
            bpe.scale = Location.FromBytes(data, 12 + 12 + 12);
            bpe.Gravity = Location.FromBytes(data, 12 + 12 + 12 + 12);
            bpe.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 12 + 12 + 12 + 12, 8));
            TheClient.SpawnEntity(bpe);
            return true;
        }
    }
}
