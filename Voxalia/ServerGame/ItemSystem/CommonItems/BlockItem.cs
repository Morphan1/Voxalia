﻿using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

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
            Location forw = player.ForwardVector();
            RayCastResult rcr;
            bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, 5, MaterialSolidity.ANY, player.IgnoreThis, out rcr);
            if (h)
            {
                if (rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                {
                    // TODO: ???
                }
                else if (player.TheRegion.GlobalTickTime - player.LastBlockPlace >= 0.2)
                {
                    Location block = new Location(rcr.HitData.Location) + new Location(rcr.HitData.Normal).Normalize() * 0.9f;
                    block = block.GetBlockLocation();
                    Material mat = player.TheRegion.GetBlockMaterial(block);
                    if (mat == Material.AIR) // TODO: IsPlaceableIn?
                    {
                        CollisionResult hit = player.TheRegion.Collision.CuboidLineTrace(new Location(0.45, 0.45, 0.45), block + new Location(0.5),
                            block + new Location(0.5, 0.5, 0.501), player.TheRegion.Collision.ShouldCollide);
                        if (!hit.Hit)
                        {
                            Material matx = (Material)item.Datum;
                            player.TheRegion.SetBlockMaterial(block, matx);
                            player.Network.SendPacket(new DefaultSoundPacketOut(block, DefaultSound.PLACE, (byte)matx.Sound()));
                            item.Count = item.Count - 1;
                            if (item.Count <= 0)
                            {
                                player.Items.RemoveItem(player.Items.cItem);
                            }
                            else
                            {
                                player.Network.SendPacket(new SetItemPacketOut(player.Items.Items.IndexOf(item), item));
                            }
                            player.LastBlockPlace = player.TheRegion.GlobalTickTime;
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
            // TODO: Possible store fist item info reference?
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
