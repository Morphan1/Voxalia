using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class ServerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base server
        // @Group General Information
        // @ReturnType ServerTag
        // @Returns the server object.
        // -->
        Server TheServer;

        public ServerTagBase(Server tserver)
        {
            Name = "server";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            data.Shrink();
            if (data.Input.Count == 0)
            {
                return new TextTag(ToString());
            }
            switch (data.Input[0])
            {
                // <--[tagbase]
                // @Name ServerTag.online_players
                // @Group General Information
                // @ReturnType ListTag<PlayerTag>
                // @Returns a list of all online players.
                // @Example .online_players could return "Fortifier|mcmonkey".
                // -->
                case "online_players":
                    ListTag players = new ListTag();
                    foreach (PlayerEntity p in TheServer.Players)
                    {
                        players.ListEntries.Add(new PlayerTag(p));
                    }
                    return players.Handle(data.Shrink());
                // <--[tagbase]
                // @Name ServerTag.loaded_regions
                // @Group General Information
                // @ReturnType ListTag<RegionTag>
                // @Returns a list of all loaded regions.
                // @Example .loaded_regions could return "default|bob".
                // -->
                case "loaded_regions":
                    ListTag regions = new ListTag();
                    foreach (Region r in TheServer.LoadedRegions)
                    {
                        regions.ListEntries.Add(new RegionTag(r));
                    }
                    return regions.Handle(data.Shrink());
                // <--[tagbase]
                // @Name ServerTag.match_player[<TextTag>]
                // @Group General Information
                // @ReturnType PlayerTag
                // @Returns the player who's name best matches the input.
                // @Example .match_player[Fort] out of a group of "Fortifier", "Fort", and "Forty" would return "Fort".
                // -->
                case "match_player":
                    string pname = data.GetModifier(0);
                    PlayerEntity player = TheServer.GetPlayerFor(pname);
                    if (player == null)
                    {
                        data.Error("Invalid player '" + TagParser.Escape(pname) + "'!");
                        return new TextTag("&{NULL}");
                    }
                    return new PlayerTag(player).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
