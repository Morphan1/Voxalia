using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using Frenetic;
using Frenetic.CommandSystem;


namespace ShadowOperations.ServerGame.EntitySystem
{
    public class TargetScriptRunnerEntity: TargetEntity
    {
        public TargetScriptRunnerEntity(Server tserver)
            : base(tserver)
        {
        }

        public override void Trigger(Entity ent, Entity user)
        {
            CommandQueue queue;
            TheServer.Commands.CommandSystem.ExecuteScript(scriptcommands, null, out queue);
        }
        public CommandScript scriptcommands = null;

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "scriptcommands":
                    scriptcommands = CommandScript.SeparateCommands("SCRIPTRUNNERFORTARGET" + Targetname, data, TheServer.Commands.CommandSystem);
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("scriptcommands", scriptcommands.FullString()));
            return vars;
        }
    }
}
