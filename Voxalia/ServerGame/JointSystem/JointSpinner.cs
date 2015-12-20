using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.Constraints;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointSpinner: BaseJoint
    {
        public JointSpinner(PhysicsEntity e1, PhysicsEntity e2, Location dir)
        {
            One = e1;
            Two = e2;
            Direction = dir;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new RevoluteAngularJoint(Ent1.Body, Ent2.Body, Direction.ToBVector());
        }
        
        public Location Direction;
    }
}
