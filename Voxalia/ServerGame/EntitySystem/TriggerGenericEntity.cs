using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.EntitySystem
{
    public class TriggerGenericEntity: TriggerEntity, EntityUseable
    {
        public TriggerGenericEntity(Location half, Server tserver)
            : base(half, tserver)
        {
        }

        public TriggerType Trigger_Type = TriggerType.USE;

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("trigger_type", Trigger_Type.ToString()));
            return vars;
        }

        public bool Use(Entity user)
        {
            if (Trigger_Type == TriggerType.USE)
            {
                Activate(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        // TODO: Implement touch!

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "trigger_type":
                    TriggerType ttype;
                    if (Enum.TryParse(data.ToUpper(), out ttype))
                    {
                        Trigger_Type = ttype;
                    }
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

    }

    public enum TriggerType : byte
    {
        NONE = 0,
        TOUCH = 1,
        USE = 2
    }
}
