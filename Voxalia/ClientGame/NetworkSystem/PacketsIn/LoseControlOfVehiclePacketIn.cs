using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class LoseControlOfVehiclePacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 8)
            {
                return false;
            }
            CharacterEntity driver = TheClient.TheRegion.GetEntity(Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8))) as CharacterEntity;
            ModelEntity vehicle = TheClient.TheRegion.GetEntity(Utilities.BytesToLong(Utilities.BytesPartial(data, 8, 8))) as ModelEntity;
            if (driver == null || vehicle == null)
            {
                return true; // Might've been despawned.
            }
            PlayerEntity player = driver as PlayerEntity;
            if (player == null)
            {
                return true; // TODO: non-player support!
            }
            player.InVehicle = false;
            player.DrivingMotors.Clear();
            player.SteeringMotors.Clear();
            player.Vehicle = null;
            vehicle.HeloPilot = null;
            vehicle.PlanePilot = null;
            return true;
        }
    }
}
