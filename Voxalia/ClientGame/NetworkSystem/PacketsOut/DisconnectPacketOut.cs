namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class DisconnectPacketOut: AbstractPacketOut
    {
        public DisconnectPacketOut()
        {
            ID = 4;
            Data = new byte[] { 0 };
        }
    }
}
