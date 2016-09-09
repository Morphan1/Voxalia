using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.JointSystem;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehiclePartEntity: ModelEntity
    {
        public double StepHeight = 0.7f;

        public bool IsWheel;

        public override EntityType GetEntityType()
        {
            return EntityType.VEHICLE_PART;
        }

        public override BsonDocument GetSaveData()
        {
            // TODO: Save *IF* detached from owner vehicle!
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
            TheRegion.AddJoint(new ConstWheelStepUp(this, StepHeight));
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
