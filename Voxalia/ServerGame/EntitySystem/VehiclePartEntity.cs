using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehiclePartEntity: ModelEntity
    {
        public float StepHeight = 0.7f;

        public bool IsWheel;

        public override EntityType GetEntityType()
        {
            return EntityType.VEHICLE_PART;
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save properly!
            return null;
        }

        public VehiclePartEntity(Region tregion, string model, bool is_wheel)
            : base(model, tregion)
        {
            IsWheel = is_wheel;
            SetFriction(3.5f);
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.Space.Add(new WheelStepUpConstraint(Body, TheRegion.Collision, StepHeight)); // TODO: Proper server constraint wrapper
        }

        public void TryToStepUp()
        {
            if (!IsWheel)
            {
                return;
            }
        }
    }
}
