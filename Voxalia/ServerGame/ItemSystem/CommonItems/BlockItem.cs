using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BlockItem: BaseItemInfo
    {
        public BlockItem()
            : base()
        {
            Name = "block";
        }

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            Location eye = player.GetEyePosition();
            CollisionResult cr = player.TheWorld.Collision.RayTrace(eye, eye + player.ForwardVector() * 5, player.IgnoreThis);
            if (cr.Hit)
            {
                if (cr.HitEnt != null)
                {
                    // TODO: ???
                }
                else if (player.TheWorld.GlobalTickTime - player.LastBlockPlace >= 0.2)
                {
                    Location block = player.TheWorld.GetBlockLocation(cr.Position + cr.Normal * 0.5);
                    Material mat = player.TheWorld.GetBlockMaterial(block);
                    if (mat == Material.AIR) // TODO: IsPlaceableIn
                    {
                        CollisionResult hit = player.TheWorld.Collision.CuboidLineTrace(new Location(0.41, 0.41, 0.41), block + new Location(0.5),
                            block + new Location(0.5, 0.5, 0.501), player.TheWorld.Collision.ShouldCollide);
                        if (!hit.Hit)
                        {
                            player.TheWorld.SetBlockMaterial(block, (Material)item.Datum);
                            item.Count = item.Count - 1;
                            if (item.Count <= 0)
                            {
                                player.Items.RemoveItem(player.Items.cItem);
                            }
                            else
                            {
                                player.Network.SendPacket(new SetItemPacketOut(player.Items.Items.IndexOf(item), item));
                            }
                            player.LastBlockPlace = player.TheWorld.GlobalTickTime;
                        }
                    }
                }
            }
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.LastBlockPlace = 0;
        }

        public override void Click(Entity entity, ItemStack item)
        {
            // TODO: Possible store fist item info reference?
            entity.TheServer.Items.Infos["fist"].Click(entity, item);
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            entity.TheServer.Items.Infos["fist"].ReleaseClick(entity, item);
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

        public override void Tick(Entity entity, ItemStack item)
        {
        }
    }
}
