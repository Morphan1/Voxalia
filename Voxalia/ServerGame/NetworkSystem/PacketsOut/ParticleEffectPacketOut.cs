using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class ParticleEffectPacketOut: AbstractPacketOut
    {
        public ParticleEffectPacketOut(ParticleEffectNetType type, float dat1, Location pos)
        {
            ID = ServerToClientPacket.PARTICLE_EFFECT;
            Data = new byte[1 + 4 + 12];
            Data[0] = (byte)type;
            Utilities.FloatToBytes(dat1).CopyTo(Data, 1);
            pos.ToBytes().CopyTo(Data, 1 + 4);
        }

        public ParticleEffectPacketOut(ParticleEffectNetType type, float dat1, Location pos, Location dat2)
        {
            ID = ServerToClientPacket.PARTICLE_EFFECT;
            Data = new byte[1 + 4 + 12 + 12];
            Data[0] = (byte)type;
            Utilities.FloatToBytes(dat1).CopyTo(Data, 1);
            pos.ToBytes().CopyTo(Data, 1 + 4);
            dat2.ToBytes().CopyTo(Data, 1 + 4 + 12);
        }
    }
}
