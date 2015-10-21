using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.OtherSystems;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehicleEntity: ModelEntity, EntityUseable
    {
        public string vehName;

        public VehicleEntity(string vehicle, Region tregion)
            : base("vehicles/" + vehicle + "_base", tregion)
        {
            vehName = vehicle;
            SetMass(100);
        }

        public bool hasWheels = false;

        List<Model3DNode> GetNodes(Model3DNode node)
        {
            List<Model3DNode> nodes = new List<Model3DNode>();
            nodes.Add(node);
            if (node.Children.Count > 0)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    nodes.AddRange(GetNodes(node.Children[i]));
                }
            }
            return nodes;
        }

        public override void Tick()
        {
            base.Tick();
            if (!hasWheels) // TODO: Efficiency. We shouldn't have to check this every tick!
            {
                Model mod = TheServer.Models.GetModel(model);
                if (mod == null) // TODO: mod should return a cube when all else fails?
                {
                    // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                    return;
                }
                Model3D scene = mod.Original;
                if (scene == null) // TODO: Scene should return a cube when all else fails?
                {
                    // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                    return;
                }
                SetOrientation(BEPUutilities.Quaternion.Identity);
                List<Model3DNode> nodes = GetNodes(scene.RootNode);
                for (int i = 0; i < nodes.Count; i++)
                {
                    string name = nodes[i].Name.ToLower();
                    if (name.Contains("wheel"))
                    {
                        // TODO
                        /*
                        Assimp.Vector3D apos;
                        Assimp.Vector3D ascale;
                        Assimp.Quaternion arot;
                        nodes[i].MatrixA.Decompose(out ascale, out arot, out apos);
                        Location pos = GetPosition() + new Location(apos.X, apos.Y, apos.Z - 1);// TODO: make the -1 not needed!)
                        ModelEntity wheel = new ModelEntity("vehicles/" + vehName + "_wheel.dae", TheRegion);
                        wheel.SetPosition(pos);
                        wheel.SetOrientation(BEPUutilities.Quaternion.Identity); // TODO: orient
                        //wheel.SetOrientation(new BEPUutilities.Quaternion(arot.X, arot.Y, arot.Z, arot.W));
                        wheel.Gravity = Gravity;
                        wheel.CGroup = CGroup;
                        wheel.SetMass(5);
                        wheel.mode = ModelCollisionMode.SPHERE;
                        TheRegion.SpawnEntity(wheel);
                        // TODO: better joints
                        JointBallSocket jbs = new JointBallSocket(this, wheel, pos);
                        //BEPUutilities.Vector3 side = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(1, 0, 0), wheel.GetOrientation());
                        BEPUutilities.Vector3 forward = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 1, 0), wheel.GetOrientation());
                        BEPUutilities.Vector3 up = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 0, 1), wheel.GetOrientation());
                        JointTwist jt = new JointTwist(this, wheel, Location.FromBVector(forward), Location.FromBVector(forward));
                        // TODO: For front wheels, remove the 'forward' JT and replace it with a motor - to allow turning!
                        JointTwist jt2 = new JointTwist(this, wheel, Location.FromBVector(up), Location.FromBVector(up));
                        TheRegion.AddJoint(jbs);
                        TheRegion.AddJoint(jt);
                        TheRegion.AddJoint(jt2);
                        BEPUutilities.Vector3 angvel = new BEPUutilities.Vector3(10, 0, 0);
                        wheel.Body.ApplyAngularImpulse(ref angvel);
                        wheel.Body.ActivityInformation.Activate();
                        */
                    }
                }
                hasWheels = true;
            }
        }

        public bool Use(Entity user)
        {
            Location pos = GetPosition() + Location.UnitZ * 3;
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).Teleport(pos);
            }
            else
            {
                user.SetPosition(pos);
            }
            user.SetOrientation(GetOrientation());
            JointForceWeld jfw = new JointForceWeld(this, user);
            TheRegion.AddJoint(jfw);
            return true;
        }
    }
}
