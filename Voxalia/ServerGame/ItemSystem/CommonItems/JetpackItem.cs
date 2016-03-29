using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class JetpackItem: GenericItem
    {
        public JetpackItem()
        {
            Name = "jetpack";
        }

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: Non-player support.
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.JPBoost = true;
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: Non-player support.
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.JPHover = true;
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            human.JPBoost = false;
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            human.JPHover = false;
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            bool has_fuel = human.ConsumeFuel(0);
            human.TheRegion.SendToVisible(human.GetPosition(), new FlagEntityPacketOut(human, EntityFlag.HAS_FUEL, has_fuel ? 1f : 0f));
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            human.JPBoost = false;
            human.JPHover = false;
        }
    }
}
