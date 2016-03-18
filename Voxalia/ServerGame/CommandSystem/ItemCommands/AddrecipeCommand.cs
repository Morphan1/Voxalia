using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.TagSystem.TagObjects;

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
            Arguments = "<result item> <required item> ...";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(entry);
            }
            ItemTag resulttag = ItemTag.For(TheServer, entry.GetArgumentObject(0));
            if (resulttag == null)
            {
                entry.Error("Invalid result item!");
                return;
            }
            ItemStack result = resulttag.Internal;
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
            TheServer.Recipes.AddRecipe(result, items.ToArray());
            if (entry.ShouldShowGood())
            {
                entry.Good("Added recipe!");
            }
        }
    }
}
