using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.JointSystem;
using BEPUutilities;
using FreneticScript;
using Voxalia.ServerGame.OtherSystems;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class VehicleEntity : ModelEntity, EntityUseable
    {
        public List<JointVehicleMotor> DrivingMotors = new List<JointVehicleMotor>();

        public List<JointVehicleMotor> SteeringMotors = new List<JointVehicleMotor>();

        public Seat DriverSeat;

        public string vehName;

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

        public VehicleEntity(string vehicle, Region tregion)
            : base("vehicles/" + vehicle + "_base", tregion)
        {
            vehName = vehicle;
            SetMass(1500);
            DriverSeat = new Seat(this, Location.UnitZ * 2);
            Seats = new List<Seat>();
            Seats.Add(DriverSeat);
        }

        public void StartUse(Entity user)
        {
            if (user.CurrentSeat == DriverSeat)
            {
                DriverSeat.Kick();
                return;
            }
            DriverSeat.Accept((PhysicsEntity)user);
        }

        public void StopUse(Entity user)
        {
            // Do nothing.
        }

        public void Accepted(CharacterEntity character)
        {
            GainControlOfVehiclePacketOut gcovpo = new GainControlOfVehiclePacketOut(character, this);
            foreach (PlayerEntity plent in TheRegion.Players)
            {
                if (plent.ShouldSeePosition(GetPosition()))
                {
                    plent.Network.SendPacket(gcovpo);
                }
            }
            // TODO: handle players coming into/out-of view of the vehicle + driver!
        }

        public abstract void HandleInput(CharacterEntity character);

        public bool hasWheels = false;

        public void HandleWheels()
        {
            if (!hasWheels)
            {
                Model mod = TheServer.Models.GetModel(model);
                if (mod == null) // TODO: mod should return a cube when all else fails?
                {
                    return;
                }
                Model3D scene = mod.Original;
                if (scene == null) // TODO: Scene should return a cube when all else fails?
                {
                    return;
                }
                SetOrientation(Quaternion.Identity);
                List<Model3DNode> nodes = GetNodes(scene.RootNode);
                List<VehiclePartEntity> frontwheels = new List<VehiclePartEntity>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    string name = nodes[i].Name.ToLowerFast();
                    if (name.Contains("wheel"))
                    {
                        Matrix mat = nodes[i].MatrixA;
                        Model3DNode tnode = nodes[i].Parent;
                        while (tnode != null)
                        {
                            mat = tnode.MatrixA * mat;
                            tnode = tnode.Parent;
                        }
                        Location pos = GetPosition() + new Location(mat.M14, mat.M34, mat.M24) + offset; // NOTE: wtf happened to this matrix?
                        VehiclePartEntity wheel = new VehiclePartEntity(TheRegion, "vehicles/" + vehName + "_wheel", true);
                        wheel.SetPosition(pos);
                        wheel.SetOrientation(Quaternion.Identity);
                        wheel.Gravity = Gravity;
                        wheel.CGroup = CGroup;
                        wheel.SetMass(30);
                        wheel.mode = ModelCollisionMode.CONVEXHULL;
                        TheRegion.SpawnEntity(wheel);
                        wheel.SetPosition(pos);
                        if (name.After("wheel").StartsWith("f"))
                        {
                            SteeringMotors.Add(ConnectWheel(wheel, false, true));
                            frontwheels.Add(wheel);
                        }
                        else if (name.After("wheel").StartsWith("b"))
                        {
                            DrivingMotors.Add(ConnectWheel(wheel, true, true));
                        }
                        else
                        {
                            ConnectWheel(wheel, true, false);
                        }
                        wheel.Body.ActivityInformation.Activate();
                    }
                }
                if (frontwheels.Count == 2)
                {
                    JointSpinner js = new JointSpinner(frontwheels[0], frontwheels[1], new Location(1, 0, 0));
                    TheRegion.AddJoint(js);
                }
                hasWheels = true;
            }
        }

        public JointVehicleMotor ConnectWheel(VehiclePartEntity wheel, bool driving, bool powered)
        {
            wheel.SetFriction(2.5f);
            Vector3 left = Quaternion.Transform(new Vector3(-1, 0, 0), wheel.GetOrientation());
            Vector3 up = Quaternion.Transform(new Vector3(0, 0, 1), wheel.GetOrientation());
            JointSlider pointOnLineJoint = new JointSlider(this, wheel, -new Location(up));
            JointLAxisLimit suspensionLimit = new JointLAxisLimit(this, wheel, 0f, 0.1f, wheel.GetPosition(), wheel.GetPosition(), -new Location(up));
            JointPullPush spring = new JointPullPush(this, wheel, -new Location(up), true);
            BEPUphysics.CollisionRuleManagement.CollisionRules.AddRule(wheel.Body, this.Body, BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase); // TODO: How necessary is this? Should we replicate this clientside?
            if (driving)
            {
                JointSpinner spinner = new JointSpinner(this, wheel, new Location(-left));
                TheRegion.AddJoint(spinner);
            }
            else
            {
                JointSwivelHinge swivelhinge = new JointSwivelHinge(this, wheel, new Location(up), new Location(-left));
                TheRegion.AddJoint(swivelhinge);
            }
            TheRegion.AddJoint(pointOnLineJoint);
            TheRegion.AddJoint(suspensionLimit);
            TheRegion.AddJoint(spring);
            if (powered)
            {
                JointVehicleMotor motor = new JointVehicleMotor(this, wheel, new Location(driving ? left : up), !driving);
                TheRegion.AddJoint(motor);
                return motor;
            }
            return null;
        }
    }
}
