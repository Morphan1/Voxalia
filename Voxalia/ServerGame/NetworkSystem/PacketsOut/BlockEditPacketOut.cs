using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class BlockEditPacketOut: AbstractPacketOut
    {
        public BlockEditPacketOut(Location[] pos, ushort[] mat, byte[] dat, byte[] paints)
        {
            ID = ServerToClientPacket.BLOCK_EDIT;
            DataStream outp = new DataStream();
            DataWriter dw = new DataWriter(outp);
            dw.WriteInt(pos.Length);
            for (int i = 0; i < pos.Length; i++)
            {
                dw.WriteBytes(pos[i].ToBytes());
            }
            for (int i = 0; i < mat.Length; i++)
            {
                dw.WriteBytes(Utilities.UshortToBytes(mat[i]));
            }
            dw.WriteBytes(dat);
            dw.WriteBytes(paints);
            dw.Flush();
            Data = outp.ToArray();
        }
    }
}
