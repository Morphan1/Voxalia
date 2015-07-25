using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class SpawnLightPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 12 + 4 + 4 + 12 + 12)
            {
                return false;
            }
            PointLightEntity ple = new PointLightEntity(TheClient.TheWorld);
            ple.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            ple.LightColor = Location.FromBytes(data, 8);
            ple.texturesize = Utilities.BytesToInt(Utilities.BytesPartial(data, 8 + 12, 4));
            if (ple.texturesize < 8 || ple.texturesize > TheClient.CVars.r_shadowquality_max.ValueI)
            {
                ple.texturesize = TheClient.CVars.r_shadowquality_max.ValueI;
            }
            ple.Radius = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 12 + 4, 4));
            if (ple.Radius < 0.1f || ple.Radius > 10000f)
            {
                ple.Radius = 10;
            }
            ple.SetPosition(Location.FromBytes(data, 8 + 12 + 4 + 4));
            ple.SetVelocity(Location.FromBytes(data, 8 + 12 + 4 + 4 + 12));
            TheClient.TheWorld.SpawnEntity(ple);
            return true;
        }
    }
}
