using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class PlayerUpdatePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 12 + 12 + 2 + 4 + 4 + 1 + 4 + 4)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid length for PlayerUpdatePacketIn!");
                return false;
            }
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            Location pos = Location.FromBytes(data, 8);
            Location vel = Location.FromBytes(data, 8 + 12);
            ushort keys = Utilities.BytesToUshort(Utilities.BytesPartial(data, 8 + 12 + 12, 2));
            float dX = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 12 + 12 + 2, 4));
            float dY = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 12 + 12 + 2 + 4, 4));
            Location ang = new Location();
            ang.Yaw = dX;
            ang.Pitch = dY;
            byte st = data[8 + 12 + 12 + 2 + 4 + 4];
            float xm = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 12 + 12 + 2 + 4 + 4 + 1, 4));
            float ym = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 12 + 12 + 2 + 4 + 4 + 1 + 4, 4));
            Stance stance = Stance.Standing;
            if (st == 1)
            {
                stance = Stance.Crouching;
            }
            for (int i = 0; i < TheClient.TheRegion.Entities.Count; i++)
            {
                if (TheClient.TheRegion.Entities[i] is CharacterEntity)
                {
                    CharacterEntity e = (CharacterEntity)TheClient.TheRegion.Entities[i];
                    if (e.EID == eID)
                    {
                        e.SetPosition(pos);
                        e.SetVelocity(vel);
                        e.Direction = ang;
                        e.Upward = (keys & 1) == 1;
                        e.Downward = (keys & 8) == 8;
                        e.CBody.StanceManager.DesiredStance = stance;
                        e.XMove = xm;
                        e.YMove = ym;
                        return true;
                    }
                }
            }
            SysConsole.Output(OutputType.WARNING, "Invalid entity (" + eID + ") for PlayerUpdatePacketIn!");
            return false;
        }
    }
}
