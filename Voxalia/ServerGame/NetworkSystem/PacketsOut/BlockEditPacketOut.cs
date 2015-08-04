using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class BlockEditPacketOut: AbstractPacketOut
    {
        public BlockEditPacketOut(Location[] pos, Material[] mat, byte[] dat)
        {
            ID = 25;
            DataStream outp = new DataStream();
            DataWriter dw = new DataWriter(outp);
            dw.WriteInt(pos.Length);
            for (int i = 0; i < pos.Length; i++)
            {
                dw.WriteBytes(pos[i].ToBytes());
            }
            for (int i = 0; i < mat.Length; i++)
            {
                dw.WriteBytes(Utilities.UshortToBytes((ushort)mat[i]));
            }
            dw.WriteBytes(dat);
            dw.Flush();
            Data = outp.ToArray();
        }
    }
}
