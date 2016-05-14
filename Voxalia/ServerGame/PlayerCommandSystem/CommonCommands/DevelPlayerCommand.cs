using System;
using System.Collections.Generic;
using System.Diagnostics;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using BEPUphysics;
using BEPUutilities;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.OtherSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript;
using Voxalia.ServerGame.TagSystem.TagObjects;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class DevelPlayerCommand : AbstractPlayerCommand
    {
        public DevelPlayerCommand()
        {
            Name = "devel";
            Silent = false;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count <= 0)
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
            string arg0 = entry.InputArguments[0];
            if (arg0 == "spawnVehicle" && entry.InputArguments.Count > 1)
            {
                VehicleEntity ve = new VehicleEntity(entry.InputArguments[1], entry.Player.TheRegion);
                ve.SetPosition(entry.Player.GetEyePosition() + entry.Player.ForwardVector() * 5);
                entry.Player.TheRegion.SpawnEntity(ve);
            }
            else if (arg0 == "fly")
            {
                if (entry.Player.IsFlying)
                {
                    entry.Player.Unfly();
                    entry.Player.Network.SendMessage("Unflying!");
                }
                else
                {
                    entry.Player.Fly();
                    entry.Player.Network.SendMessage("Flying!");
                }
            }
            else if (arg0 == "playerDebug")
            {
                entry.Player.Network.SendMessage("YOU: " + entry.Player.Name + ", tractionForce: " + entry.Player.CBody.TractionForce
                     + ", mass: " + entry.Player.CBody.Body.Mass + ", radius: " + entry.Player.CBody.BodyRadius + ", hasSupport: " + entry.Player.CBody.SupportFinder.HasSupport
                     + ", hasTraction: " + entry.Player.CBody.SupportFinder.HasTraction + ", isAFK: " + entry.Player.IsAFK + ", timeAFK: " + entry.Player.TimeAFK);
            }
            else if (arg0 == "chunkDebug")
            {
                Biome biome;
                Location posBlock = entry.Player.GetPosition().GetBlockLocation();
                float h = entry.Player.TheRegion.Generator.GetHeight(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2, entry.Player.TheRegion.Seed3,
                    entry.Player.TheRegion.Seed4, entry.Player.TheRegion.Seed5, (float)posBlock.X, (float)posBlock.Y, (float)posBlock.Z, out biome);
                BlockInternal bi = entry.Player.TheRegion.GetBlockInternal_NoLoad((entry.Player.GetPosition() + new Location(0, 0, -0.05f)).GetBlockLocation());
                entry.Player.Network.SendMessage("Mat: " + bi.Material + ", data: " + ((int)bi.BlockData) + ", locDat: " + ((int)bi.BlockLocalData)
                    + ", Damage: " + bi.Damage + ", Paint: " + bi.BlockPaint
                    + ", xp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXP() + ", xm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXM()
                    + ", yp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYP() + ", ym: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYM()
                    + ", zp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesTOP() + ", zm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesBOTTOM());
                float temp = entry.Player.TheRegion.BiomeGen.GetTemperature(entry.Player.TheRegion.Seed2, entry.Player.TheRegion.Seed3, (float)posBlock.X, (float)posBlock.Y);
                float down = entry.Player.TheRegion.BiomeGen.GetDownfallRate(entry.Player.TheRegion.Seed3, entry.Player.TheRegion.Seed4, (float)posBlock.X, (float)posBlock.Y);
                entry.Player.Network.SendMessage("Height: " + h + ", temperature: " + temp + ", downfallrate: " + down + ", biome yield: " + biome.GetName());
            }
            else if (arg0 == "structureSelect" && entry.InputArguments.Count > 1)
            {
                string arg1 = entry.InputArguments[1];
                entry.Player.Items.GiveItem(new ItemStack("structureselector", arg1, entry.Player.TheServer, 1, "items/admin/structure_selector",
                    "Structure Selector", "Selects and creates a '" + arg1 + "' structure!", System.Drawing.Color.White, "items/admin/structure_selector", false));
            }
            else if (arg0 == "structureCreate" && entry.InputArguments.Count > 1)
            {
                string arg1 = entry.InputArguments[1];
                entry.Player.Items.GiveItem(new ItemStack("structurecreate", arg1, entry.Player.TheServer, 1, "items/admin/structure_create",
                    "Structure Creator", "Creates a '" + arg1 + "' structure!", System.Drawing.Color.White, "items/admin/structure_create", false));
            }
            else if (arg0 == "musicBlock" && entry.InputArguments.Count > 3)
            {
                int arg1 = Utilities.StringToInt(entry.InputArguments[1]);
                float arg2 = Utilities.StringToFloat(entry.InputArguments[2]);
                float arg3 = Utilities.StringToFloat(entry.InputArguments[3]);
                entry.Player.Items.GiveItem(new ItemStack("customblock", entry.Player.TheServer, 1, "items/custom_blocks/music_block",
                    "Music Block", "Plays music!", System.Drawing.Color.White, "items/custom_blocks/music_block", false,
                    new KeyValuePair<string, TemplateObject>("music_type", new IntegerTag(arg1)),
                    new KeyValuePair<string, TemplateObject>("music_volume", new NumberTag(arg2)),
                    new KeyValuePair<string, TemplateObject>("music_pitch", new NumberTag(arg3)))
                {
                    Datum = new BlockInternal((ushort)Material.DEBUG, 0, 0, 0).GetItemDatum()
                });
            }
            else if (arg0 == "structurePaste" && entry.InputArguments.Count > 1)
            {
                string arg1 = entry.InputArguments[1];
                entry.Player.Items.GiveItem(new ItemStack("structurepaste", arg1, entry.Player.TheServer, 1, "items/admin/structure_paste",
                    "Structor Paster", "Pastes a ;" + arg1 + "; structure!", System.Drawing.Color.White, "items/admin/structure_paste", false));
            }
            else if (arg0 == "spawnSmallPlant" && entry.InputArguments.Count > 1)
            {
                entry.Player.TheRegion.SpawnSmallPlant(entry.InputArguments[1].ToLowerFast(), entry.Player.GetPosition());
            }
            else if (arg0 == "spawnTree" && entry.InputArguments.Count > 1)
            {
                entry.Player.TheRegion.SpawnTree(entry.InputArguments[1].ToLowerFast(), entry.Player.GetPosition());
            }
            else if (arg0 == "spawnTarget")
            {
                TargetEntity te = new TargetEntity(entry.Player.TheRegion);
                te.SetPosition(entry.Player.GetPosition() + entry.Player.ForwardVector() * 5);
                te.TheRegion.SpawnEntity(te);
            }
            else if (arg0 == "spawnSlime" && entry.InputArguments.Count > 2)
            {
                SlimeEntity se = new SlimeEntity(entry.Player.TheRegion, Utilities.StringToFloat(entry.InputArguments[2]));
                se.mod_color = ColorTag.For(entry.InputArguments[1]).Internal;
                se.SetPosition(entry.Player.GetPosition() + entry.Player.ForwardVector() * 5);
                se.TheRegion.SpawnEntity(se);
            }
            else if (arg0 == "timePathfind" && entry.InputArguments.Count > 1)
            {
                double dist = Utilities.StringToDouble(entry.InputArguments[1]);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<Location> locs = entry.Player.TheRegion.FindPath(entry.Player.GetPosition(), entry.Player.GetPosition() + new Location(dist, 0, 0), dist * 2, 1.5f);
                sw.Stop();
                if (locs != null)
                {
                    entry.Player.Network.SendPacket(new PathPacketOut(locs));
                }
                entry.Player.Network.SendMessage("Took " + sw.ElapsedMilliseconds + "ms, passed: " + (locs != null));
            }
            else if (arg0 == "findPath")
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Location eye = entry.Player.GetEyePosition();
                Location forw = entry.Player.ForwardVector();
                Location goal;
                RayCastResult rcr;
                if (entry.Player.TheRegion.SpecialCaseRayTrace(eye, forw, 50, MaterialSolidity.FULLSOLID, entry.Player.IgnorePlayers, out rcr))
                {
                    goal = new Location(rcr.HitData.Location);
                }
                else
                {
                    goal = eye + forw * 50;
                }
                List<Location> locs = entry.Player.TheRegion.FindPath(entry.Player.GetPosition(), goal, 50 * 2, 1.5f);
                sw.Stop();
                if (locs != null)
                {
                    entry.Player.Network.SendPacket(new PathPacketOut(locs));
                }
                entry.Player.Network.SendMessage("Took " + sw.ElapsedMilliseconds + "ms, passed: " + (locs != null));
            }
            else if (arg0 == "gameMode" && entry.InputArguments.Count > 1)
            {
                GameMode mode;
                if (Enum.TryParse(entry.InputArguments[1].ToUpperInvariant(), out mode))
                {
                    entry.Player.Mode = mode;
                }
            }
            else if (arg0 == "teleport" && entry.InputArguments.Count > 1)
            {
                entry.Player.Teleport(Location.FromString(entry.InputArguments[1]));
            }
            else if (arg0 == "tickRate")
            {
                entry.Player.Network.SendMessage("Intended tick rate: " + entry.Player.TheServer.CVars.g_fps.ValueI + ", actual tick rate (last second): " + entry.Player.TheServer.TPS);
            }
            else if (arg0 == "paintBrush" && entry.InputArguments.Count > 1)
            {
                ItemStack its = entry.Player.TheServer.Items.GetItem("tools/paintbrush");
                byte col = Colors.ForName(entry.InputArguments[1]);
                its.Datum = col;
                its.DrawColor = Colors.ForByte(col);
                entry.Player.Items.GiveItem(its);
            }
            else if (arg0 == "paintBomb" && entry.InputArguments.Count > 1)
            {
                ItemStack its = entry.Player.TheServer.Items.GetItem("weapons/grenades/paintbomb", 10);
                byte col = Colors.ForName(entry.InputArguments[1]);
                its.Datum = col;
                its.DrawColor = Colors.ForByte(col);
                entry.Player.Items.GiveItem(its);
            }
            else if (arg0 == "sledgeHammer" && entry.InputArguments.Count > 1)
            {
                ItemStack its = entry.Player.TheServer.Items.GetItem("tools/sledgehammer");
                int bsd = BlockShapeRegistry.GetBSDFor(entry.InputArguments[1]);
                its.Datum = bsd;
                entry.Player.Items.GiveItem(its);
            }
            else if (arg0 == "blockDamage" && entry.InputArguments.Count > 1)
            {
                BlockDamage damage;
                if (Enum.TryParse(entry.InputArguments[1], out damage))
                {
                    Location posBlock = (entry.Player.GetPosition() + new Location(0, 0, -0.05f)).GetBlockLocation();
                    BlockInternal bi = entry.Player.TheRegion.GetBlockInternal(posBlock);
                    bi.Damage = damage;
                    entry.Player.TheRegion.SetBlockMaterial(posBlock, bi);
                }
                else
                {
                    entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                }
            }
            else
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
        }
    }
}
