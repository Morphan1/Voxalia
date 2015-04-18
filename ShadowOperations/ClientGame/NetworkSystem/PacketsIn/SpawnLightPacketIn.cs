using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class SpawnLightPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 12 + 4 + 4 + 12 + 12)
            {
                return false;
            }
            PointLightEntity ple = new PointLightEntity(TheClient);
            ple.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            ple.Color = Location.FromBytes(data, 8);
            ple.texturesize = Utilities.BytesToInt(Utilities.BytesPartial(data, 8 + 12, 4));
            if (ple.texturesize < 8 || ple.texturesize > 1024)
            {
                ple.texturesize = 256;
            }
            ple.Radius = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 12 + 4, 4));
            if (ple.Radius < 0.1f || ple.Radius > 10000f)
            {
                ple.Radius = 10;
            }
            ple.SetPosition(Location.FromBytes(data, 8 + 12 + 4 + 4));
            ple.SetVelocity(Location.FromBytes(data, 8 + 12 + 4 + 4 + 12));
            TheClient.SpawnEntity(ple);
            return true;
        }
    }
}
