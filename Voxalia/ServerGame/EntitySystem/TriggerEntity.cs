using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class TriggerEntity : CuboidalEntity
    {
        public TriggerEntity(Location halfsize, Server tserver)
            : base(halfsize, tserver, false, 0)
        {
            NetworkMe = false;
        }

        public override void Recalculate()
        {
            base.Recalculate();
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.CollisionInformation.CollisionRules.Group = TheServer.Collision.Trigger; // TODO: Getter, Setter
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
