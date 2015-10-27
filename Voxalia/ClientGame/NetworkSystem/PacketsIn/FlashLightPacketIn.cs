using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class FlashLightPacketIn: AbstractPacketIn
    {
        public void Destroy(Entity ent)
        {
            SpotLight sl = null;
            if (ent is PlayerEntity)
            {
                sl = ((PlayerEntity)ent).Flashlight;
                ((PlayerEntity)ent).Flashlight = null;
            }
            else
            {
                sl = ((OtherPlayerEntity)ent).Flashlight;
                ((OtherPlayerEntity)ent).Flashlight = null;
            }
            if (sl != null)
            {
                sl.Destroy();
                TheClient.Lights.Remove(sl);
            }
        }

        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 1 + 4 + 12)
            {
                return false;
            }
            long EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            bool enabled = (data[8] & 1) == 1;
            float distance = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 1, 4));
            Location color = Location.FromBytes(data, 8 + 1 + 4);
            Entity ent = TheClient.TheRegion.GetEntity(EID);
            if (ent == null || !(ent is PlayerEntity || ent is OtherPlayerEntity))
            {
                return false;
            }
            Destroy(ent);
            if (enabled)
            {
                SpotLight sl = new SpotLight(ent.GetPosition(), TheClient.CVars.r_shadowquality_flashlight.ValueI, distance, color, Location.UnitX, 45);
                if (ent is PlayerEntity)
                {
                    sl.Direction = ((PlayerEntity)ent).ForwardVector();
                    sl.Reposition(ent.GetPosition());
                    ((PlayerEntity)ent).Flashlight = sl;
                }
                else
                {
                    sl.Direction = ((OtherPlayerEntity)ent).ForwardVector();
                    sl.Reposition(ent.GetPosition());
                    ((OtherPlayerEntity)ent).Flashlight = sl;
                }
                TheClient.Lights.Add(sl);
            }
            return true;
        }
    }
}
