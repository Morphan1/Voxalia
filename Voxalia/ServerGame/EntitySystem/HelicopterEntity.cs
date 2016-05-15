using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;
using BEPUutilities;

namespace Voxalia.ServerGame.EntitySystem
{
    public class HelicopterEntity : VehicleEntity
    {
        public HelicopterEntity(string heli, Region tregion)
            : base(heli, tregion)
        {
        }

        public override EntityType GetEntityType()
        {
            return EntityType.HELICOPTER;
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save properly!
            return null;
        }

        public bool ILeft = false;
        public bool IRight = false;

        public float ForwBack = 0;
        public float RightLeft = 0;

        public override void SpawnBody()
        {
            base.SpawnBody();
        }

        public float LiftStrength
        {
            get
            {
                return GetMass() + 500;
            }
        }

        public float EstimatedMass
        {
            get
            {
                float basic = GetMass();
                foreach (Seat seat in Seats)
                {
                    if (seat.Sitter != null)
                    {
                        basic += seat.Sitter.GetMass();
                    }
                }
                return basic;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (DriverSeat.Sitter == null)
            {
                return;
            }
            Location up = new Location(Quaternion.Transform(Vector3.UnitZ, GetOrientation()));
            if (ILeft)
            {
                ApplyForce(up * LiftStrength * 16.0 * TheRegion.Delta);
            }
            else if (IRight)
            {
                ApplyForce(up * LiftStrength * 9.0 * TheRegion.Delta);
            }
            else
            {
                ApplyForce(up * EstimatedMass * 9.8 * (3.0 / 2.0) * TheRegion.Delta);
            }
            Body.ModifyLinearDamping(0.3f);
            Body.ModifyAngularDamping(0.3f);
            Vector3 adj = new Vector3(ForwBack, RightLeft, 0f);
            Vector3 res = Quaternion.Transform(adj, GetOrientation());
            ApplyForce(Location.UnitZ, new Location(res) * TheRegion.Delta * 8f);
            Vector3 vup = Quaternion.Transform(Vector3.UnitZ, GetOrientation());
            ApplyForce(Location.UnitZ, (vup.Z >= 0 ? 4f : -8f) * new Location(vup.X, vup.Y, 0));
        }

        public override void HandleInput(CharacterEntity character)
        {
            ILeft = character.ItemLeft;
            IRight = character.ItemRight;
            ForwBack = character.YMove;
            RightLeft = character.XMove;
        }
    }
}
