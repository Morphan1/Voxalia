using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehiclePartEntity: ModelEntity
    {

        public override EntityType GetEntityType()
        {
            return EntityType.VEHICLE_PART;
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save properly!
            return null;
        }

        public VehiclePartEntity(Region tregion, string model)
            : base(model, tregion)
        {
        }
    }
}
