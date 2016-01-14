using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class GainControlOfVehiclePacketOut: AbstractPacketOut
    {
        public GainControlOfVehiclePacketOut(CharacterEntity character, VehicleEntity vehicle)
        {
            ID = 34;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteLong(character.EID);
            dw.WriteInt(vehicle.DrivingMotors.Count);
            dw.WriteInt(vehicle.SteeringMotors.Count);
            for (int i = 0; i < vehicle.DrivingMotors.Count; i++)
            {
                dw.WriteLong(vehicle.DrivingMotors[i].JID);
            }
            for (int i = 0; i < vehicle.SteeringMotors.Count; i++)
            {
                dw.WriteLong(vehicle.SteeringMotors[i].JID);
            }
            dw.Flush();
            Data = ds.ToArray();
            dw.Close();
        }
    }
}
