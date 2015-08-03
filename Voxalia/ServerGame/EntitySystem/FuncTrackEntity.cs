using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    class FuncTrackEntity: CubeEntity, EntityTargettable
    {
        public FuncTrackEntity(Location half, Region tworld, float mass)
            : base(half, tworld, mass)
        {
        }

        public string Target = "";

        public string Targetname = "";

        public float MinDistance = 0.1f;

        public int LoopsPerActivation = 0;

        public float MoveSpeed = 1f;

        public string GetTargetName()
        {
            return Targetname;
        }

        public void Trigger(Entity ent, Entity user)
        {
            if (active)
            {
                return;
            }
            loopsSoFar = 0;
            List<Entity> ents = TheWorld.GetTargets(Target);
            if (ents.Count == 0)
            {
                return;
            }
            t1 = ents[0];
            relpos = GetPosition() - t1.GetPosition();
            if (!(ents[0] is TargetPositionEntity))
            {
                return;
            }
            ents = TheWorld.GetTargets(((TargetPositionEntity)t1).Target);
            if (ents.Count == 0)
            {
                return;
            }
            nextInLine = ents[0];
            active = true;
        }

        public bool active = false;
        public int loopsSoFar = 0;
        public Entity t1 = null;
        public Location relpos = Location.Zero;
        public Entity nextInLine = null;
        // TODO: Track orientation / rotate with the positions?
        Location pforward;

        public override void Tick()
        {
            if (active)
            {
                if (!nextInLine.IsSpawned)
                {
                    active = false;
                }
                else
                {
                    if (((nextInLine.GetPosition() + relpos) - GetPosition()).LengthSquared() <= MinDistance * MinDistance)
                    {
                        SetPosition(nextInLine.GetPosition() + relpos);
                        SetVelocity(Location.Zero);
                        pforward = Location.Zero;
                        if (nextInLine == t1)
                        {
                            loopsSoFar++;
                            if (loopsSoFar >= LoopsPerActivation && LoopsPerActivation > 0)
                            {
                                active = false;
                            }
                        }
                        List<Entity> ents = TheWorld.GetTargets(((TargetPositionEntity)nextInLine).Target);
                        if (ents.Count == 0)
                        {
                            SysConsole.Output(OutputType.WARNING, "Incomplete FuncTrack chain!"); // TODO: Debug mode only
                            active = false;
                        }
                        nextInLine = ents[0];
                    }
                    if (active)
                    {
                        Location target = nextInLine.GetPosition() + relpos;
                        Location forward = (target - GetPosition()).Normalize();
                        if (forward == -pforward)
                        {
                            SysConsole.Output(OutputType.INFO, "FuncTrack: Tracking to force-stop."); // TODO: Debug mode only
                            SetPosition(target);
                            SetVelocity(Location.Zero);
                        }
                        else
                        {
                            SetVelocity(forward * MoveSpeed);
                        }
                        pforward = forward;
                    }
                }
            }
            base.Tick();
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "target":
                    Target = data;
                    return true;
                case "targetname":
                    Targetname = data;
                    return true;
                case "mindistance":
                    MinDistance = Utilities.StringToFloat(data);
                    return true;
                case "loopsperactivation":
                    LoopsPerActivation = Utilities.StringToInt(data);
                    return true;
                case "movespeed":
                    MoveSpeed = Utilities.StringToFloat(data);
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("target", Target));
            vars.Add(new KeyValuePair<string, string>("targetname", Targetname));
            vars.Add(new KeyValuePair<string, string>("mindistance", MinDistance.ToString()));
            vars.Add(new KeyValuePair<string, string>("loopsperactivation", LoopsPerActivation.ToString()));
            vars.Add(new KeyValuePair<string, string>("movespeed", MoveSpeed.ToString()));
            return vars;
        }
    }
}
