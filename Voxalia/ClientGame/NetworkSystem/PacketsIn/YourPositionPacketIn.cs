﻿using System;
using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class YourPositionPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 12 + 12 + 1 + 8)
            {
                return false;
            }
            long ID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            Location pos = Location.FromBytes(data, 8);
            Location vel = Location.FromBytes(data, 8 + 12);
            double gtt = Utilities.BytesToDouble(Utilities.BytesPartial(data, 8 + 12 + 12 + 1, 8));
            TheClient.Player.PacketFromServer(gtt, ID, pos, vel);
            TheClient.Player.CBody.StanceManager.DesiredStance = data[8 + 12 + 12] == 0 ? Stance.Standing : Stance.Crouching; // TODO: Handle better!
            return true;
        }
    }
}
