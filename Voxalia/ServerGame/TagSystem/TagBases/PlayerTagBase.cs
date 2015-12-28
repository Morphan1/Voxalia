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
        Server TheServer;

        // <--[tag]
        // @Base player[<TextTag>]
        // @Group Entities
        // @ReturnType PlayerTag
        // @Returns the player with the given name.
        // -->
        public PlayerTagBase(Server tserver)
        {
            Name = "player";
            TheServer = tserver;
        }

        public override string Handle(TagData data)
        {
            string pname = data.GetModifier(0).ToLower();
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
                    if (player.Name.ToLower() == pname)
                    {
                        return new PlayerTag(player).Handle(data.Shrink());
                    }
                }
            }
            return new TextTag("&{NULL}").Handle(data.Shrink());
        }
    }
}
