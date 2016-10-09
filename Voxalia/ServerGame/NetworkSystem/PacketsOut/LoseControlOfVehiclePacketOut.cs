//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
