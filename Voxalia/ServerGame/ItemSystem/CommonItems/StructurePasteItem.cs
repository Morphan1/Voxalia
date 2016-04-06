using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics;
using Voxalia.ServerGame.WorldSystem;


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
            if (!Program.Files.Exists("structures/" + item.SecondaryName + ".str")) // TODO: structure helper engine
            {
                throw new Exception("File does not exist!"); // TODO: Handle better.
            }
            Structure structure = new Structure(Program.Files.ReadBytes("structures/" + item.SecondaryName + ".str"));
            player.Pasting = structure.ToBGE(player.TheRegion, player.GetPosition());
            player.TheRegion.SpawnEntity(player.Pasting);
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            // TODO: Should non-players be allowed here?
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.Pasting != null)
            {
                Location eye = player.GetEyePosition();
                Location forw = player.ForwardVector();
                player.Pasting.SetPosition((eye + forw * 5).GetBlockLocation());
                RayCastResult rcr;
                bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, 5, MaterialSolidity.ANY, player.IgnoreThis, out rcr);
                if (h)
                {
                    if (rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                    {
                        // TODO: ???
                    }
                    else if (player.TheRegion.GlobalTickTime - player.LastBlockBreak >= 0.2)
                    {
                        Location block = (new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01).GetBlockLocation();
                        Material mat = player.TheRegion.GetBlockMaterial(block);
                        if (mat != Material.AIR)
                        {
                            player.Pasting.SetPosition(block);
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
            // TODO: Generic 'player.gettargetblock'?
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
                else if (player.TheRegion.GlobalTickTime - player.LastBlockBreak >= 0.2)
                {
                    Location block = new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01;
                    Material mat = player.TheRegion.GetBlockMaterial(block);
                    if (mat != Material.AIR)
                    {
                        try
                        {
                            if (!Program.Files.Exists("structures/" + item.SecondaryName + ".str")) // TODO: structure helper engine
                            {
                                throw new Exception("File does not exist!"); // TODO: Handle better.
                            }
                            Structure structure = new Structure(Program.Files.ReadBytes("structures/" + item.SecondaryName + ".str"));
                            structure.Paste(player.TheRegion, block);
                            player.Network.SendMessage("^2Pasted structure at " + block + ", with offset of " + structure.Origin.X + "," + structure.Origin.Y + "," + structure.Origin.Z);
                            // TODO: Pasting sound of some form!
                            player.LastBlockBreak = player.TheRegion.GlobalTickTime;
                        }
                        catch (Exception ex)
                        {
                            player.Network.SendMessage("^1Failed to paste structure: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}
