﻿using FreneticScript.TagHandlers;
using Voxalia.ClientGame.CommandSystem.TagObjects.EntityTagObjects;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.TagBases
{
    public class PlayerTagBase: TemplateTagBase
    {
        // <--[tag]
        // @Base player
        // @Game VoxaliaClient
        // @Group Entities
        // @ReturnType PlayerTag
        // @Returns the primary game player object.
        // -->

        public Client TheClient;

        /// <summary>
        /// Construct the PlayerTags - for internal use only.
        /// </summary>
        public PlayerTagBase(Client tclient)
        {
            Name = "player";
            TheClient = tclient;
        }

        /// <summary>
        /// Handles a 'player' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject Handle(TagData data)
        {
            return new PlayerTag(TheClient).Handle(data.Shrink());
        }
    }
}