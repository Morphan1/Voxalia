using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.ClientMainSystem;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using FreneticScript.TagHandlers;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    public class CdevelCommand: AbstractCommand
    {
        public Client TheClient;

        public CdevelCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "cdevel";
            Description = "Clientside developmental commands.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            switch (entry.GetArgument(queue, 0))
            {
                case "userName":
                    {
                        if (entry.Arguments.Count > 1)
                        {
                            TheClient.Username = entry.GetArgument(queue, 1);
                            entry.Good(queue, "Set.");
                        }
                        else
                        {
                            entry.Info(queue, "Your username is: " + TagParser.Escape(TheClient.Username));
                        }
                        break;
                    }
                case "lightDebug":
                    {
                        Location pos = TheClient.Player.GetPosition();
                        pos.Z = pos.Z + 1;
                        double XP = Math.Floor(pos.X / Chunk.CHUNK_SIZE);
                        double YP = Math.Floor(pos.Y / Chunk.CHUNK_SIZE);
                        double ZP = Math.Floor(pos.Z / Chunk.CHUNK_SIZE);
                        int x = (int)(Math.Floor(pos.X) - (XP * Chunk.CHUNK_SIZE));
                        int y = (int)(Math.Floor(pos.Y) - (YP * Chunk.CHUNK_SIZE));
                        int z = (int)(Math.Floor(pos.Z) - (ZP * Chunk.CHUNK_SIZE));
                        while (true)
                        {
                            Chunk ch = TheClient.TheRegion.GetChunk(new Location(XP, YP, ZP));
                            if (ch == null)
                            {
                                entry.Good(queue, "Passed with flying light sources!");
                                goto end;
                            }
                            while (z < Chunk.CHUNK_SIZE)
                            {
                                if (ch.GetBlockAt((int)x, (int)y, (int)z).IsOpaque())
                                {
                                    entry.Good(queue, "Died: " + x + ", " + y + ", " + z + " -- " + XP + ", " + YP + ", " + ZP);
                                    goto end;
                                }
                                z++;
                            }
                            ZP++;
                            z = 0;
                        }
                        end:
                        break;
                    }
                default:
                    ShowUsage(queue, entry);
                    break;
            }
        }
    }
}
