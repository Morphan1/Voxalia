using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehicleEntity: ModelEntity
    {
        public string vehName;

        public VehicleEntity(string vehicle, World tworld)
            : base("vehicles/" + vehicle + "_base.dae", tworld)
        {
            vehName = vehicle;
            SetMass(100);
        }

        public bool hasWheels = false;

        public override void Tick()
        {
            base.Tick();
            if (!hasWheels) // TODO: Efficiency. We shouldn't have to check this every tick!
            {
                Assimp.Scene scene = TheServer.Models.GetModel(model).Original;
                SetOrientation(BEPUutilities.Quaternion.Identity);
                for (int i = 0; i < scene.Meshes.Count; i++)
                {
                    for (int x = 0; x < scene.Meshes[i].Bones.Count; x++)
                    {
                        string name = scene.Meshes[i].Bones[x].Name.ToLower();
                        if (name.Contains("wheel"))
                        {
                            Assimp.Vector3D apos;
                            Assimp.Vector3D ascale;
                            Assimp.Quaternion arot;
                            Assimp.Matrix4x4 mat = scene.RootNode.Transform;
                            mat.Inverse();
                            (scene.Meshes[i].Bones[x].OffsetMatrix * mat).Decompose(out ascale, out arot, out apos);
                            Location pos = GetPosition() + new Location(apos.X, apos.Y, apos.Z);
                            ModelEntity wheel = new ModelEntity("vehicles/" + vehName + "_wheel.dae", TheWorld);
                            wheel.SetPosition(pos);
                            wheel.SetOrientation(BEPUutilities.Quaternion.Identity); // TODO: orient
                            //wheel.SetOrientation(new BEPUutilities.Quaternion(arot.X, arot.Y, arot.Z, arot.W));
                            wheel.Gravity = Gravity;
                            wheel.CGroup = CGroup;
                            wheel.SetMass(5);
                            wheel.mode = ModelCollisionMode.SPHERE;
                            TheWorld.SpawnEntity(wheel);
                            // TODO: better joints
                            JointBallSocket jbs = new JointBallSocket(this, wheel, pos);
                            //BEPUutilities.Vector3 side = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(1, 0, 0), wheel.GetOrientation());
                            BEPUutilities.Vector3 forward = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 1, 0), wheel.GetOrientation());
                            BEPUutilities.Vector3 up = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 0, 1), wheel.GetOrientation());
                            JointTwist jt = new JointTwist(this, wheel, Location.FromBVector(forward), Location.FromBVector(forward));
                            JointTwist jt2 = new JointTwist(this, wheel, Location.FromBVector(up), Location.FromBVector(up));
                            TheWorld.AddJoint(jbs);
                            TheWorld.AddJoint(jt);
                            TheWorld.AddJoint(jt2);
                            BEPUutilities.Vector3 angvel = new BEPUutilities.Vector3(10, 0, 0);
                            wheel.Body.ApplyAngularImpulse(ref angvel);
                            wheel.Body.ActivityInformation.Activate();
                        }
                    }
                }
                hasWheels = true;
            }
        }
    }
}
