using System;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

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
            if (arg0 == "spawnVehicle")
            {
                VehicleEntity ve = new VehicleEntity("failmobile", entry.Player.TheRegion);
                ve.SetPosition(entry.Player.GetEyePosition() + entry.Player.ForwardVector() * 5);
                entry.Player.TheRegion.SpawnEntity(ve);
            }
            else if (arg0 == "chunkDebug")
            {
                float h = entry.Player.TheRegion.Generator.GetHeight(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2,
                    (float)Math.Floor(entry.Player.GetPosition().X), (float)Math.Floor(entry.Player.GetPosition().Y));
                Location chunkpos = entry.Player.TheRegion.ChunkLocFor(entry.Player.GetPosition()) * 30 + new Location(15);
                float hC = entry.Player.TheRegion.Generator.GetHeight(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2, (float)chunkpos.X, (float)chunkpos.Y);
                entry.Player.Network.SendMessage("Generate height: " + h + " ==  " + Math.Round(h));
                BlockInternal bi = entry.Player.TheRegion.GetBlockInternal_NoLoad((entry.Player.GetPosition() + new Location(0, 0, -0.1)).GetBlockLocation());
                entry.Player.Network.SendMessage("Mat: " + ((Material)bi.BlockMaterial) + ", data: " + ((int)bi.BlockData)
                    + ", xp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXP() + ", xm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXM()
                    + ", yp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYP() + ", ym: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYM()
                    + ", zp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesTOP() + ", zm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesBOTTOM());
                float temp = entry.Player.TheRegion.BiomeGen.GetTemperature(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2, (float)chunkpos.X, (float)chunkpos.Y);
                float down = entry.Player.TheRegion.BiomeGen.GetDownfallRate(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2, (float)chunkpos.X, (float)chunkpos.Y);
                Biome biome = entry.Player.TheRegion.BiomeGen.BiomeFor(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2, (float)chunkpos.X, (float)chunkpos.Y, (float)chunkpos.Z, hC);
                entry.Player.Network.SendMessage("Chunk height: " + hC + ", temperature: " + temp + ", downfallrate: " + down + ", biome yield: " + biome.GetName());
            }
            else
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
        }
    }
}
