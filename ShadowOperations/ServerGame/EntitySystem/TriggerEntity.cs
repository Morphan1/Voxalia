using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public abstract class TriggerEntity : CuboidalEntity
    {
        public TriggerEntity(Location halfsize, Server tserver)
            : base(halfsize, tserver, false, 0)
        {
            NetworkMe = false;
            Shape.CollisionInformation.CollisionRules.Group = TheServer.Collision.Trigger;
        }

        public override void Recalculate()
        {
            base.Recalculate();
            Shape.CollisionInformation.CollisionRules.Group = TheServer.Collision.Trigger;
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.CollisionInformation.CollisionRules.Group = TheServer.Collision.Trigger;
        }

        public string Target = "";

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "target":
                    Target = data;
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public void Activate(Entity activator)
        {
            TheServer.Trigger(Target, this, activator);
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("target", Target));
            return vars;
        }
    }
}
