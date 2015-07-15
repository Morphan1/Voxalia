using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BowItem: BaseItemInfo
    {
        public BowItem()
        {
            Name = "bow";
        }

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }

        public float Speed = 10;

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            ArrowEntity ae = new ArrowEntity(player.TheWorld);
            ae.SetPosition(player.GetEyePosition());
            ae.NoCollide.Add(player.EID);
            Location forward = player.ForwardVector();
            ae.SetVelocity(forward * 10);
            BEPUutilities.Matrix lookatlh = Utilities.LookAtLH(Location.Zero, forward, Location.UnitZ);
            lookatlh.Transpose();
            ae.Angles = BEPUutilities.Quaternion.CreateFromRotationMatrix(lookatlh);
            ae.Angles *= BEPUutilities.Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.UnitX, 90f * (float)Utilities.PI180);
            player.TheWorld.SpawnEntity(ae);
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
        }

        public override void Use(Entity entity, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }
    }
}
