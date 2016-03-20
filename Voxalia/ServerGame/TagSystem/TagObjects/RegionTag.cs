using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class RegionTag : TemplateObject
    {
        // <--[object]
        // @Type RegionTag
        // @SubType TextTag
        // @Group Entities
        // @Description Represents any Region.
        // -->
        Region Internal;

        public RegionTag(Region reg)
        {
            Internal = reg;
        }

        public override TemplateObject Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return this;
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name RegionTag.name
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the region's name.
                // @Example "default" .name returns "default".
                // -->
                case "name":
                    return new TextTag(Internal.Name).Handle(data.Shrink());
                // <--[tag]
                // @Name RegionTag.ram_usage_chunks
                // @Group Statistics
                // @ReturnType TextTag
                // @Returns how much RAM is used by the blocks in chunks in this region.
                // @Example "default" .ram_usage_chunks might return "1000000".
                // -->
                case "ram_usage_chunks":
                    return new IntegerTag((long)Chunk.RAM_USAGE * (long)Internal.LoadedChunks.Count).Handle(data.Shrink());
                // <--[tag]
                // @Name RegionTag.ram_usage_entities
                // @Group Statistics
                // @ReturnType TextTag
                // @Returns (badly) estimates how much RAM is used by the entities in this region.
                // @Example "default" .ram_usage_entities might return "1000000".
                // -->
                case "ram_usage_entities":
                    {
                        long sum = 0;
                        foreach (Entity ent in Internal.Entities)
                        {
                            sum += ent.GetRAMUsage();
                        }
                        return new IntegerTag(sum).Handle(data.Shrink());
                    }
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.Name;
        }
    }
}
