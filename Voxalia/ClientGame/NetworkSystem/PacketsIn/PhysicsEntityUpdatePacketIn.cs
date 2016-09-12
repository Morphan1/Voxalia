using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class PhysicsEntityUpdatePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 24 + 24 + 16 + 24 + 1 + 8)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid physentupdtpacket: invalid length!");
                return false;
            }
            Location pos = Location.FromDoubleBytes(data, 0);
            Location vel = Location.FromDoubleBytes(data, 24);
            BEPUutilities.Quaternion ang = Utilities.BytesToQuaternion(data, 24 + 24);
            Location angvel = Location.FromDoubleBytes(data, 24 + 24 + 16);
            bool active = (data[24 + 24 + 16 + 24] & 1) == 1;
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 24 + 24 + 16 + 24 + 1, 8));
            for (int i = 0; i < TheClient.TheRegion.Entities.Count; i++)
            {
                if (TheClient.TheRegion.Entities[i] is PhysicsEntity)
                {
                    PhysicsEntity e = (PhysicsEntity)TheClient.TheRegion.Entities[i];
                    if (e.EID == eID)
                    {
                        if (e is ModelEntity && ((ModelEntity)e).PlanePilot == TheClient.Player)
                        {
                            float lerp = TheClient.CVars.n_ourvehiclelerp.ValueF;
                            e.SetPosition(e.GetPosition() + (pos - e.GetPosition()) * lerp);
                            e.SetVelocity(e.GetVelocity() + (vel - e.GetVelocity()) * lerp);
                            e.SetAngularVelocity(e.GetAngularVelocity() + (angvel - e.GetAngularVelocity()) * lerp);
                            e.SetOrientation(BEPUutilities.Quaternion.Slerp(e.GetOrientation(), ang, lerp));
                        }
                        else
                        {
                            e.SetPosition(pos);
                            e.SetVelocity(vel);
                            e.SetOrientation(ang);
                            e.SetAngularVelocity(angvel);
                        }
                        if (e.Body != null && e.Body.ActivityInformation != null && e.Body.ActivityInformation.IsActive && !active) // TODO: Why are the first two checks needed?
                        {
                            if (e.Body.ActivityInformation.SimulationIsland != null) // TODO: Why is this needed?
                            {
                                e.Body.ActivityInformation.SimulationIsland.IsActive = false;
                            }
                        }
                        else if (e.Body != null && e.Body.ActivityInformation != null && !e.Body.ActivityInformation.IsActive && active) // TODO: Why are the first two checks needed?
                        {
                            e.Body.ActivityInformation.Activate();
                        }
                        return true;
                    }
                }
            }
            TheClient.Network.SendPacket(new PleaseRedefinePacketOut(eID));
            return true;
        }
    }
}
