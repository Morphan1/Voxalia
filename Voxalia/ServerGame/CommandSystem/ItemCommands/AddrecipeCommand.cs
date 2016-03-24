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
using FreneticScript.TagHandlers;

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
            MinimumArguments = 1;
            MaximumArguments = -1;
        }

        public override void Execute(CommandEntry entry)
        {
            TemplateObject cb = entry.GetArgumentObject(0);
            if (cb.ToString() == "\0CALLBACK")
            {
                return;
            }
            if (entry.InnerCommandBlock == null)
            {
                entry.Error("Invalid or missing command block!");
                return;
            }
            ListTag mode = ListTag.For(cb);
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
            TheServer.Recipes.AddRecipe(RecipeRegistry.ModeFor(mode), entry.InnerCommandBlock, entry.BlockStart, items.ToArray());
            entry.Queue.CommandIndex = entry.BlockEnd + 2;
            if (entry.ShouldShowGood())
            {
                entry.Good("Added recipe!");
            }
        }
    }
}
