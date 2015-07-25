using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PointLightEntity: PrimitiveEntity
    {
        public int TextureSize = 256;
        public Location Color = Location.One;
        public float Radius = 10;

        public PointLightEntity(World tworld)
            : base(tworld)
        {
            network = false; // TODO: Maybe network?
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("texture_size", TextureSize.ToString()));
            vars.Add(new KeyValuePair<string, string>("color", Color.ToString()));
            vars.Add(new KeyValuePair<string, string>("radius", Radius.ToString()));
            return vars;
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "texture_size":
                    TextureSize = Utilities.StringToInt(data);
                    return true;
                case "radius":
                    Radius = Utilities.StringToFloat(data);
                    return true;
                case "color":
                    Color = Location.FromString(data);
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }
    }
}
