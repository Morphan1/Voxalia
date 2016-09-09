using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class CharacterUpdatePacketOut: AbstractPacketOut
    {
        public CharacterUpdatePacketOut(CharacterEntity player)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.CHARACTER_UPDATE;
            Data = new byte[8 + 12 + 12 + 2 + 4 + 4 + 1 + 4 + 4 + 4];
            Utilities.LongToBytes(player.EID).CopyTo(Data, 0);
            player.GetPosition().ToBytes().CopyTo(Data, 8);
            player.GetVelocity().ToBytes().CopyTo(Data, 8 + 12);
            ushort dat = (ushort)((player.Upward ? 1 : 0) | (player.Downward ? 8 : 0));
            Utilities.UshortToBytes(dat).CopyTo(Data, 8 + 12 + 12);
            Utilities.FloatToBytes((float)player.Direction.Yaw).CopyTo(Data, 8 + 12 + 12 + 2);
            Utilities.FloatToBytes((float)player.Direction.Pitch).CopyTo(Data, 8 + 12 + 12 + 2 + 4);
            Data[8 + 12 + 12 + 2 + 4 + 4] = (byte)(player.IsCrouching ? 1 : 0);
            Utilities.FloatToBytes((float)player.XMove).CopyTo(Data, 8 + 12 + 12 + 2 + 4 + 4 + 1);
            Utilities.FloatToBytes((float)player.YMove).CopyTo(Data, 8 + 12 + 12 + 2 + 4 + 4 + 1 + 4);
            Utilities.FloatToBytes((float)player.SprintOrWalk).CopyTo(Data, 8 + 12 + 12 + 2 + 4 + 4 + 1 + 4 + 4);
        }
    }
}
