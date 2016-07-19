using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.EntitySystem;
using BEPUphysics.CollisionRuleManagement;

namespace Voxalia.ClientGame.JointSystem
{
    public class JointNoCollide : BaseFJoint
    {
        public JointNoCollide(PhysicsEntity e1, PhysicsEntity e2)
        {
            One = e1;
            Two = e2;
        }

        public override void Enable()
        {
            CollisionRules.AddRule(((PhysicsEntity)One).Body.CollisionInformation, ((PhysicsEntity)Two).Body.CollisionInformation, CollisionRule.NoBroadPhase);
            CollisionRules.AddRule(((PhysicsEntity)Two).Body.CollisionInformation, ((PhysicsEntity)One).Body.CollisionInformation, CollisionRule.NoBroadPhase);
            base.Enable();
        }

        public override void Disable()
        {
            CollisionRules.RemoveRule(((PhysicsEntity)One).Body.CollisionInformation, ((PhysicsEntity)Two).Body.CollisionInformation);
            CollisionRules.RemoveRule(((PhysicsEntity)Two).Body.CollisionInformation, ((PhysicsEntity)One).Body.CollisionInformation);
            base.Disable();
        }

        public override void Solve()
        {
            // Do nothing.
        }
    }
}
