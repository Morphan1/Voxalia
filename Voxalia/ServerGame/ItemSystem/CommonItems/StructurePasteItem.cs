//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics;
using Voxalia.ServerGame.WorldSystem;
using BEPUutilities;


namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    class StructurePasteItem: GenericItem
    {
        public StructurePasteItem()
        {
            Name = "structurepaste";
        }
        
        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            // TODO: Should non-players be allowed here?
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.Pasting != null)
            {
                player.Pasting.RemoveMe();
                player.Pasting = null;
            }
            player.PastingDist = 5;
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
            // TODO: Should non-players be allowed here?
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.Pasting != null)
            {
                player.Pasting.RemoveMe();
                player.Pasting = null;
            }
            player.PastingDist = 5;
            if (!entity.TheServer.Files.Exists("structures/" + item.SecondaryName + ".str")) // TODO: structure helper engine
            {
                throw new Exception("File does not exist!"); // TODO: Handle better.
            }
            Structure structure = new Structure(entity.TheServer.Files.ReadBytes("structures/" + item.SecondaryName + ".str"));
            player.Pasting = structure.ToBGE(player.TheRegion, player.GetPosition());
            player.TheRegion.SpawnEntity(player.Pasting);
        }

        public static void RotateAround(BlockGroupEntity bge, int angle)
        {
            bge.Angle = (bge.Angle + angle) % 360;
            if (bge.Angle < 0)
            {
                bge.Angle += 360;
            }
            Location relpos = new Location(bge.shapeOffs.X, bge.shapeOffs.Y, 0);
            if (bge.Angle == 0)
            {
                bge.rotOffs = Location.Zero;
            }
            else if (bge.Angle == 90)
            {
                bge.rotOffs = new Location(-bge.shapeOffs.Y, bge.shapeOffs.X, 0) - relpos;
            }
            else if (bge.Angle == 180)
            {
                bge.rotOffs = new Location(-bge.shapeOffs.X, -bge.shapeOffs.Y, 0) - relpos;
            }
            else if (bge.Angle == 270)
            {
                bge.rotOffs = new Location(bge.shapeOffs.Y, -bge.shapeOffs.X, 0) - relpos;
            }
            else
            {
                bge.Angle = 0;
                bge.rotOffs = Location.Zero;
            }
            bge.SetOrientation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (double)bge.Angle * (double)Utilities.PI180));
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            // TODO: Should non-players be allowed here?
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.ItemUp)
            {
                player.PastingDist += (double)(player.TheRegion.Delta * 3.0);
                player.PastingDist = Math.Min(player.PastingDist, 20);
            }
            if (player.ItemDown)
            {
                player.PastingDist -= (double)(player.TheRegion.Delta * 3.0);
                player.PastingDist = Math.Max(player.PastingDist, 0.1f);
            }
            if (player.Pasting != null)
            {
                if (player.ItemLeft && !player.WasItemLefting)
                {
                    RotateAround(player.Pasting, 90);
                }
                if (player.ItemRight && !player.WasItemRighting)
                {
                    RotateAround(player.Pasting, -90);
                }
                Location eye = player.GetEyePosition();
                Location forw = player.ForwardVector();
                player.Pasting.SetPosition((eye + forw * player.PastingDist).GetBlockLocation() - player.Pasting.Origin - player.Pasting.shapeOffs);
                RayCastResult rcr;
                bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, player.PastingDist, MaterialSolidity.ANY, player.IgnoreThis, out rcr);
                if (h)
                {
                    if (rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                    {
                        // TODO: ???
                    }
                    else
                    {
                        Location block = (new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01).GetBlockLocation();
                        Material mat = player.TheRegion.GetBlockMaterial(block);
                        if (mat != Material.AIR)
                        {
                            player.Pasting.SetPosition(block - player.Pasting.Origin - player.Pasting.shapeOffs);
                        }
                    }
                }
            }
        }

        public override void Click(Entity entity, ItemStack item)
        {
            // TODO: Should non-players be allowed here?
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.TheRegion.GlobalTickTime - player.LastBlockBreak < 0.2)
            {
                return;
            }
           // TODO: Generic 'player.gettargetblock'?
            Location eye = player.GetEyePosition();
            Location forw = player.ForwardVector();
            RayCastResult rcr;
            bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, player.PastingDist, MaterialSolidity.ANY, player.IgnoreThis, out rcr);
            Location pasteloc = (eye + forw * player.PastingDist + player.Pasting.rotOffs).GetBlockLocation();
            if (h)
            {
                if (rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                {
                    // TODO: ???
                }
                else
                {
                    Location block = (new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01).GetBlockLocation();
                    Material mat = player.TheRegion.GetBlockMaterial(block);
                    if (mat != Material.AIR)
                    {
                        pasteloc = block + (player.Pasting != null ? player.Pasting.rotOffs : Location.Zero);
                    }
                }
            }
            try
            {
                if (!entity.TheServer.Files.Exists("structures/" + item.SecondaryName + ".str")) // TODO: structure helper engine
                {
                    throw new Exception("File does not exist!"); // TODO: Handle better.
                }
                Structure structure = new Structure(entity.TheServer.Files.ReadBytes("structures/" + item.SecondaryName + ".str"));
                int ang = (player.Pasting != null ? player.Pasting.Angle : 0);
                structure.Paste(player.TheRegion, pasteloc, ang);
                player.SendMessage(TextChannel.COMMAND_RESPONSE, "^2Pasted structure at " + pasteloc + ", with offset of " + structure.Origin.X + "," + structure.Origin.Y + "," + structure.Origin.Z + " at angle " + ang);
                // TODO: Pasting sound of some form! And particles!
                player.LastBlockBreak = player.TheRegion.GlobalTickTime;
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                player.SendMessage(TextChannel.COMMAND_RESPONSE, "^1Failed to paste structure: " + ex.Message);
            }
        }
    }
}
