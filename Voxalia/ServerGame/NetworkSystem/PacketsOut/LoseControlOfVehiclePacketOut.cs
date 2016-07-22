using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class LoseControlOfVehiclePacketOut : AbstractPacketOut
    {
        public LoseControlOfVehiclePacketOut(CharacterEntity driver, VehicleEntity vehicle)
        {
            ID = ServerToClientPacket.LOSE_CONTROL_OF_VEHICLE;
            Data = new byte[8 + 8];
            Utilities.LongToBytes(driver.EID).CopyTo(Data, 0);
            Utilities.LongToBytes(vehicle.EID).CopyTo(Data, 8);
        }
    }
}
