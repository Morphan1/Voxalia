using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript.TagHandlers;
using Voxalia.ClientGame.ClientMainSystem;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ClientGame.CommandSystem.TagBases
{
    public class AudioTagBase : TemplateTagBase
    {
        public Client TheClient;
        
        public AudioTagBase(Client tclient)
        {
            Name = "audio";
            TheClient = tclient;
        }
        
        public override TemplateObject Handle(TagData data)
        {
            return new AudioTag() { TheClient = TheClient }.Handle(data.Shrink());
        }

        class AudioTag : TemplateObject
        {
            public Client TheClient;

            public override TemplateObject Handle(TagData data)
            {
                if (data.Remaining == 0)
                {
                    return this;
                }
                switch (data[0])
                {
                    case "microphone_bytes":
                        return new IntegerTag(TheClient.Sounds.Microphone.stat_bytes).Handle(data.Shrink());
                    default:
                        return new TextTag(ToString()).Handle(data);
                }
            }
        }
    }
}
