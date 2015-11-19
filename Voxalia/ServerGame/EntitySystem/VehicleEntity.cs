using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.OtherSystems;
using BEPUutilities;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehicleEntity: ModelEntity, EntityUseable
    {
        public string vehName;
        public Seat DriverSeat;

        public VehicleEntity(string vehicle, Region tregion)
            : base("vehicles/" + vehicle + "_base", tregion)
        {
            vehName = vehicle;
            SetMass(100);
            DriverSeat = new Seat(this, Location.UnitZ * 2);
            Seats = new List<Seat>();
            Seats.Add(DriverSeat);
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
                        Matrix mat = nodes[i].MatrixA;
                        Model3DNode tnode = nodes[i].Parent;
                        while (tnode != null)
                        {
                            mat = tnode.MatrixA * mat;
                            tnode = tnode.Parent;
                        }
                        Location pos = GetPosition() + new Location(mat.M14, mat.M34, mat.M24); // TODO: Why are the matrices transposed?! // TODO: Why are Y and Z flipped?!
                        ModelEntity wheel = new ModelEntity("vehicles/" + vehName + "_wheel", TheRegion);
                        wheel.SetPosition(pos);
                        wheel.SetOrientation(Quaternion.Identity); // TOOD: orient
                        wheel.Gravity = Gravity;
                        wheel.CGroup = CGroup;
                        wheel.SetMass(5);
                        wheel.mode = ModelCollisionMode.SPHERE;
                        TheRegion.SpawnEntity(wheel);
                        //JointBallSocket jbs = new JointBallSocket(this, wheel, pos);
                        BEPUutilities.Vector3 side = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(1, 0, 0), wheel.GetOrientation());
                        //BEPUutilities.Vector3 forward = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 1, 0), wheel.GetOrientation());
                        BEPUutilities.Vector3 down = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 0, -1), wheel.GetOrientation());
                        JointSlider js = new JointSlider(this, wheel, new Location(down));
                        JointSpinner jsp = new JointSpinner(wheel, this, new Location(side));
                        TheRegion.AddJoint(js);
                        TheRegion.AddJoint(jsp);
                        //JointTwist jt = new JointTwist(this, wheel, new Location(forward), new Location(forward));
                        // TODO: For front wheels, remove the 'forward' JT and replace it with a motor - to allow turning!
                        //JointTwist jt2 = new JointTwist(this, wheel, new Location(up), new Location(up));
                        //TheRegion.AddJoint(jbs);
                        //TheRegion.AddJoint(jt);
                        //TheRegion.AddJoint(jt2);
                        BEPUutilities.Vector3 angvel = new BEPUutilities.Vector3(10, 0, 0);
                        wheel.Body.ApplyAngularImpulse(ref angvel);
                        wheel.Body.ActivityInformation.Activate();
                    }
                }
                hasWheels = true;
            }
            base.Tick();
        }

        public bool Use(Entity user)
        {
            if (user.CurrentSeat == DriverSeat)
            {
                DriverSeat.Kick();
                return true;
            }
            return DriverSeat.Accept(user);
        }
    }
}
