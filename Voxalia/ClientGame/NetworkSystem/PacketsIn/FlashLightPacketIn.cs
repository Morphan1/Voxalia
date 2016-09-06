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
            if (ent is CharacterEntity)
            {
                sl = ((CharacterEntity)ent).Flashlight;
                ((CharacterEntity)ent).Flashlight = null;
            }
            if (sl != null)
            {
                sl.Destroy();
                TheClient.MainWorldView.Lights.Remove(sl);
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
            if (ent == null || !(ent is PlayerEntity || ent is CharacterEntity))
            {
                return false;
            }
            Destroy(ent);
            if (enabled)
            {
                SpotLight sl = new SpotLight(ent.GetPosition(), distance, color, Location.UnitX, 45);
                if (ent is CharacterEntity)
                {
                    sl.Direction = ((CharacterEntity)ent).ForwardVector();
                    sl.Reposition(ent.GetPosition());
                    ((CharacterEntity)ent).Flashlight = sl;
                }
                TheClient.MainWorldView.Lights.Add(sl);
            }
            return true;
        }
    }
}
