using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    class SpawnLightPacketOut: AbstractPacketOut
    {
        public SpawnLightPacketOut(PointLightEntity ple)
        {
            ID = 4;
            Data = new byte[8 + 12 + 4 + 4 + 12 + 12];
            Utilities.LongToBytes(ple.EID).CopyTo(Data, 0);
            ple.Color.ToBytes().CopyTo(Data, 8);
            Utilities.IntToBytes(ple.TextureSize).CopyTo(Data, 8 + 12);
            Utilities.FloatToBytes(ple.Radius).CopyTo(Data, 8 + 12 + 4);
            ple.GetPosition().ToBytes().CopyTo(Data, 8 + 12 + 4 + 4);
            ple.GetVelocity().ToBytes().CopyTo(Data, 8 + 12 + 4 + 4 + 12);
        }
    }
}
