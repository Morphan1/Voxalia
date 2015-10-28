using Voxalia.ClientGame.ClientMainSystem;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;

namespace Voxalia.ClientGame.CommandSystem.TagObjects.EntityTagObjects
{
    public class PlayerTag: TemplateObject
    {
        public Client TheClient;

        public PlayerTag(Client tclient)
        {
            TheClient = tclient;
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name PlayerTag.held_item_slot
                // @Group Inventory
                // @ReturnType TextTag
                // @Returns the slot of the item the player is currently holding (in their QuickBar).
                // -->
                case "held_item_slot":
                    return new TextTag(TheClient.QuickBarPos).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
