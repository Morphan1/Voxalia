using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    class PlayerUpdatePacketOut: AbstractPacketOut
    {
        public PlayerUpdatePacketOut(PlayerEntity player)
        {
            ID = 6;
            Data = new byte[8 + 12 + 12 + 2 + 4 + 4 + 1];
            Utilities.LongToBytes(player.EID).CopyTo(Data, 0);
            player.GetPosition().ToBytes().CopyTo(Data, 8);
            player.GetVelocity().ToBytes().CopyTo(Data, 8 + 12);
            ushort dat = (ushort)((player.Forward ? 1 : 0) | (player.Backward ? 2 : 0) | (player.Leftward ? 4 : 0)
                | (player.Rightward ? 8 : 0) | (player.Upward ? 16 : 0));
            Utilities.UshortToBytes(dat).CopyTo(Data, 8 + 12 + 12);
            Utilities.FloatToBytes((float)player.Direction.Yaw).CopyTo(Data, 8 + 12 + 12 + 2);
            Utilities.FloatToBytes((float)player.Direction.Pitch).CopyTo(Data, 8 + 12 + 12 + 2 + 4);
            Data[8 + 12 + 12 + 2 + 4] = (byte)(player.Stance == PlayerStance.STAND ? 0 : (player.Stance == PlayerStance.CROUCH ? 1 : 2));
        }
    }
}
