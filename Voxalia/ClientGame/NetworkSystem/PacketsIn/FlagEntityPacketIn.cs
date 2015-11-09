using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class FlagEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 1 + 4)
            {
                return false;
            }
            long eid = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            Entity e = TheClient.TheRegion.GetEntity(eid);
            if (e == null)
            {
                SysConsole.Output(OutputType.WARNING, "Flagged unknown entity!");
                return true; // Don't die, could be a minor flaw.
            }
            EntityFlag flag = (EntityFlag)data[8];
            float value = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 1, 4));
            switch (flag)
            {
                case EntityFlag.FLYING:
                    if (e is PlayerEntity)
                    {
                        if (value > 0.5f)
                        {
                            ((PlayerEntity)e).Fly();
                        }
                        else
                        {
                            ((PlayerEntity)e).Unfly();
                        }
                    }
                    else if (e is OtherPlayerEntity)
                    {
                        if (value > 0.5f)
                        {
                            ((OtherPlayerEntity)e).Fly();
                        }
                        else
                        {
                            ((OtherPlayerEntity)e).Unfly();
                        }
                    }
                    else
                    {
                        throw new Exception("Flagged non-player entity as flying!");
                    }
                    break;
                case EntityFlag.MASS:
                    if (e is PhysicsEntity)
                    {
                        ((PhysicsEntity)e).SetMass(value);
                    }
                    else
                    {
                        throw new Exception("Flagged non-physics entity with a specific mass!");
                    }
                    break;
                default:
                    throw new Exception("Unknown flag!");
            }

            return true;
        }
    }
}
