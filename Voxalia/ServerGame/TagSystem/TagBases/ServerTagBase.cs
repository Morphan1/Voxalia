using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.ItemSystem;

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
            if (data.Remaining == 0)
            {
                return new TextTag(ToString());
            }
            switch (data[0])
            {
                // <--[tagbase]
                // @Name ServerTag.online_players
                // @Group General Information
                // @ReturnType ListTag
                // @Returns a list of all online players.
                // @Example .online_players could return "Fortifier|mcmonkey".
                // -->
                case "online_players":
                    {
                        ListTag players = new ListTag();
                        foreach (PlayerEntity p in TheServer.Players)
                        {
                            players.ListEntries.Add(new PlayerTag(p));
                        }
                        return players.Handle(data.Shrink());
                    }
                // <--[tagbase]
                // @Name ServerTag.loaded_regions
                // @Group General Information
                // @ReturnType ListTag
                // @Returns a list of all loaded regions.
                // @Example .loaded_regions could return "default|bob".
                // -->
                case "loaded_regions":
                    {
                        ListTag regions = new ListTag();
                        foreach (Region r in TheServer.LoadedRegions)
                        {
                            regions.ListEntries.Add(new RegionTag(r));
                        }
                        return regions.Handle(data.Shrink());
                    }
                // <--[tagbase]
                // @Name ServerTag.loaded_recipes
                // @Group General Information
                // @ReturnType ListTag
                // @Returns a list of all loaded recipes.
                // -->
                case "loaded_recipes":
                    {
                        ListTag recipes = new ListTag();
                        foreach (ItemRecipe r in TheServer.Recipes.Recipes)
                        {
                            recipes.ListEntries.Add(new RecipeTag(r));
                        }
                        return recipes.Handle(data.Shrink());
                    }
                // <--[tagbase]
                // @Name ServerTag.can_craft_from[<ListTag>]
                // @Group General Information
                // @ReturnType ListTag
                // @Returns a list of all loaded recipes that can be crafted from the given input.
                // @Example .can_craft_from[blocks/grass_forest] could return "1&pipeblocks/grass_forest|".
                // -->
                case "can_craft_from":
                    {
                        // TODO: Handle errors neatly!
                        List<ItemStack> items = new List<ItemStack>();
                        ListTag list = ListTag.For(data.GetModifierObject(0));
                        foreach (TemplateObject obj in list.ListEntries)
                        {
                            items.Add(ItemTag.For(TheServer, obj).Internal);
                        }
                        ListTag recipes = new ListTag();
                        foreach (RecipeResult r in TheServer.Recipes.CanCraftFrom(items.ToArray()))
                        {
                            recipes.ListEntries.Add(new RecipeResultTag(r));
                        }
                        return recipes.Handle(data.Shrink());
                    }
                // <--[tagbase]
                // @Name ServerTag.match_player[<TextTag>]
                // @Group General Information
                // @ReturnType PlayerTag
                // @Returns the player whose name best matches the input.
                // @Example .match_player[Fort] out of a group of "Fortifier", "Fort", and "Forty" would return "Fort".
                // @Example .match_player[monk] out of a group of "mcmonkey", "morph", and "Fort" would return "mcmonkey".
                // -->
                case "match_player":
                    {
                        string pname = data.GetModifier(0);
                        PlayerEntity player = TheServer.GetPlayerFor(pname);
                        if (player == null)
                        {
                            data.Error("Invalid player '" + TagParser.Escape(pname) + "'!");
                            return new NullTag();
                        }
                        return new PlayerTag(player).Handle(data.Shrink());
                    }
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
