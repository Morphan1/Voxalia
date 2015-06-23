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
                        if (scene.Meshes[i].Bones[x].Name.ToLower().Contains("wheel"))
                        {
                            Assimp.Vector3D apos;
                            Assimp.Vector3D ascale;
                            Assimp.Quaternion arot;
                            scene.Meshes[i].Bones[x].OffsetMatrix.Decompose(out apos, out arot, out ascale);
                            Location pos = GetPosition() + new Location(apos.X, apos.Y, apos.Z);
                            ModelEntity wheel = new ModelEntity("vehicles/" + vehName + "_wheel.dae", TheWorld);
                            wheel.SetPosition(pos);
                            wheel.SetOrientation(new BEPUutilities.Quaternion(arot.X, arot.Y, arot.Z, arot.W));
                            wheel.Gravity = Gravity;
                            wheel.CGroup = CGroup;
                            wheel.SetMass(5);
                            TheWorld.SpawnEntity(wheel);
                            //BEPUutilities.Vector3 forward = BEPUutilities.Quaternion.Transform(new BEPUutilities.Vector3(0, 1, 0), wheel.GetOrientation());
                            //JointSpinner jbs = new JointSpinner(this, wheel, Location.FromBVector(forward));
                            JointBallSocket jbs = new JointBallSocket(this, wheel, pos);
                            TheWorld.AddJoint(jbs);
                        }

                    }
                }
                hasWheels = true;
            }
        }
    }
}
