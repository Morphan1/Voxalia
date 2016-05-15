using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class VehicleEntity : ModelEntity, EntityUseable
    {
        public Seat DriverSeat;

        public string vehName;

        public VehicleEntity(string vehicle, Region tregion)
            : base("vehicles/" + vehicle + "_base", tregion)
        {
            vehName = vehicle;
            SetMass(1500);
            DriverSeat = new Seat(this, Location.UnitZ * 2);
            Seats = new List<Seat>();
            Seats.Add(DriverSeat);
        }

        public void StartUse(Entity user)
        {
            if (user.CurrentSeat == DriverSeat)
            {
                DriverSeat.Kick();
                return;
            }
            DriverSeat.Accept((PhysicsEntity)user);
        }

        public void StopUse(Entity user)
        {
            // Do nothing.
        }

        public void Accepted(CharacterEntity character)
        {
            GainControlOfVehiclePacketOut gcovpo = new GainControlOfVehiclePacketOut(character, this);
            foreach (PlayerEntity plent in TheRegion.Players)
            {
                if (plent.ShouldSeePosition(GetPosition()))
                {
                    plent.Network.SendPacket(gcovpo);
                }
            }
            // TODO: handle players coming into/out-of view of the vehicle + driver!
        }

        public abstract void HandleInput(CharacterEntity character);
    }
}
