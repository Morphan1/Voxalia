using System.Collections.Generic;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BreadcrumbItem: BaseItemInfo
    {
        public BreadcrumbItem()
        {
            Name = "breadcrumb";
        }

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void Click(Entity entity, ItemStack item)
        {
        }

        public static double MaxRadius = 50;

        public override void AltClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            // TODO: Handle non-players
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Breadcrumbs.Clear();
            player.Breadcrumbs.Add(player.GetPosition().GetBlockLocation() + new Location(0.5f, 0.5f, 0.5f));
            // TODO: Effect?
        }

        public static double fireRate = 1f;

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
            // TODO: Handle non-players
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.TheRegion.GlobalTickTime - player.LastGunShot < fireRate)
            {
                return;
            }
            player.LastGunShot = player.TheRegion.GlobalTickTime;
            for (int i = 1; i < player.Breadcrumbs.Count; i++)
            {
                if ((player.GetPosition() - player.Breadcrumbs[i]).LengthSquared() < 4)
                {
                    player.Breadcrumbs.RemoveRange(i, player.Breadcrumbs.Count - i);
                }
            }
            List<Location> locs = new List<Location>();
            Location cpos = player.GetPosition();
            for (int i = player.Breadcrumbs.Count - 1; i >= 0; i--)
            {
                if ((player.Breadcrumbs[i] - cpos).LengthSquared() > MaxRadius * MaxRadius)
                {
                    break;
                }
                locs.Add(player.Breadcrumbs[i]);
            }
            if (locs.Count > 0)
            {
                player.Network.SendPacket(new PathPacketOut(locs));
            }
        }

        public override void Use(Entity entity, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            // TODO: Handle non-players
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.LastGunShot = 0;
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }
    }
}
