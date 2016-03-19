using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.TagSystem.TagObjects;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ServerGame.CommandSystem.ItemCommands
{
    public class AddrecipeCommand : AbstractCommand
    {
        public Server TheServer;

        public AddrecipeCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "addrecipe";
            Description = "Adds a recipe to be crafted.";
            Arguments = "<mode> <input item> ...";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(entry);
            }
            if (entry.Block == null)
            {
                entry.Error("Invalid or missing command block!");
                return;
            }
            ListTag mode = ListTag.For(entry.GetArgumentObject(0));
            List<ItemStack> items = new List<ItemStack>();
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                ItemTag required = ItemTag.For(TheServer, entry.GetArgumentObject(i));
                if (required == null)
                {
                    entry.Error("Invalid required item!");
                    return;
                }
                items.Add(required.Internal);
            }
            TheServer.Recipes.AddRecipe(RecipeRegistry.ModeFor(mode), entry.Block, items.ToArray());
            if (entry.ShouldShowGood())
            {
                entry.Good("Added recipe!");
            }
        }
    }
}
