using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class TargetPositionEntity: TargetEntity
    {
        public TargetPositionEntity(Region tregion)
            : base(tregion)
        {
        }

        public string Target = "";

        public float Modifier = 1f;

        public override void Trigger(Entity ent, Entity user)
        {
            // Do Nothing
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "target":
                    Target = data;
                    return true;
                case "modifier":
                    Modifier = Utilities.StringToFloat(data);
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("target", Target));
            vars.Add(new KeyValuePair<string, string>("modifier", Modifier.ToString()));
            return vars;
        }
    }
}
