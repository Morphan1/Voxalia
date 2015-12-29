using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class PlayerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base player[<PlayerTag>]
        // @Group Entities
        // @ReturnType PlayerTag
        // @Returns the player with the given name or entity ID.
        // -->
        Server TheServer;

        public PlayerTagBase(Server tserver)
        {
            Name = "player";
            TheServer = tserver;
        }

        public override string Handle(TagData data)
        {
            string pname = data.GetModifier(0).ToLowerInvariant();
            long pid;
            if (long.TryParse(pname, out pid))
            {
                foreach (PlayerEntity player in TheServer.Players)
                {
                    if (player.EID == pid)
                    {
                        return new PlayerTag(player).Handle(data.Shrink());
                    }
                }
            }
            else
            {
                foreach (PlayerEntity player in TheServer.Players)
                {
                    if (player.Name.ToLowerInvariant() == pname)
                    {
                        return new PlayerTag(player).Handle(data.Shrink());
                    }
                }
            }
            data.Error("Invalid player '" + TagParser.Escape(pname) + "'!");
            return "&{NULL}";
        }
    }
}
