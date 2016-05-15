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
            if (vehicle is CarEntity)
            {
                Setup(character, (CarEntity)vehicle);
            }
            else if (vehicle is HelicopterEntity)
            {
                Setup(character, (HelicopterEntity)vehicle);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void Setup(CharacterEntity character, CarEntity vehicle)
        {
            ID = 34;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteLong(character.EID);
            dw.WriteByte(0);
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

        private void Setup(CharacterEntity character, HelicopterEntity vehicle)
        {
            ID = 34;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteLong(character.EID);
            dw.WriteByte(1);
            dw.Flush();
            Data = ds.ToArray();
            dw.Close();
        }
    }
}
