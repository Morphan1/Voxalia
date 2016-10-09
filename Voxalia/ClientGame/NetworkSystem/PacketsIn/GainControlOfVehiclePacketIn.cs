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
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.ClientGame.JointSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class GainControlOfVehiclePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            long PEID = dr.ReadLong();
            byte type = dr.ReadByte();
            Entity e = TheClient.TheRegion.GetEntity(PEID);
            if (type == 0)
            {
                if (e is PlayerEntity)
                {
                    ((PlayerEntity)e).InVehicle = true;
                    int drivecount = dr.ReadInt();
                    int steercount = dr.ReadInt();
                    PlayerEntity player = (PlayerEntity)e;
                    player.DrivingMotors.Clear();
                    player.SteeringMotors.Clear();
                    for (int i = 0; i < drivecount; i++)
                    {
                        long jid = dr.ReadLong();
                        JointVehicleMotor jvm = (JointVehicleMotor)TheClient.TheRegion.GetJoint(jid);
                        if (jvm == null)
                        {
                            dr.Close();
                            return false;
                        }
                        player.DrivingMotors.Add(jvm);
                    }
                    for (int i = 0; i < steercount; i++)
                    {
                        long jid = dr.ReadLong();
                        JointVehicleMotor jvm = (JointVehicleMotor)TheClient.TheRegion.GetJoint(jid);
                        if (jvm == null)
                        {
                            dr.Close();
                            return false;
                        }
                        player.SteeringMotors.Add(jvm);
                    }
                    dr.Close();
                    return true;
                }
                // TODO: other CharacterEntity's
            }
            else if (type == 1)
            {
                if (e is PlayerEntity)
                {
                    long heloid = dr.ReadLong();
                    Entity helo = TheClient.TheRegion.GetEntity(heloid);
                    if (!(helo is ModelEntity))
                    {
                        dr.Close();
                        return false;
                    }
                    ((PlayerEntity)e).InVehicle = true;
                    ((PlayerEntity)e).Vehicle = helo;
                    ModelEntity helomod = (ModelEntity)helo;
                    helomod.TurnIntoHelicopter((PlayerEntity)e);
                    dr.Close();
                    return true;
                }
                // TODO: other CharacterEntity's
                dr.Close();
                return true;
            }
            else if (type == 2)
            {
                if (e is PlayerEntity)
                {
                    long planeid = dr.ReadLong();
                    Entity plane = TheClient.TheRegion.GetEntity(planeid);
                    if (!(plane is ModelEntity))
                    {
                        dr.Close();
                        return false;
                    }
                    ((PlayerEntity)e).InVehicle = true;
                    ((PlayerEntity)e).Vehicle = plane;
                    ModelEntity planemod = (ModelEntity)plane;
                    planemod.TurnIntoPlane((PlayerEntity)e);
                    dr.Close();
                    return true;
                }
                // TODO: other CharacterEntity's
                dr.Close();
                return true;
            }
            dr.Close();
            return false;
        }
    }
}
