using System;
using System.Collections.Generic;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.OtherSystems;
using BEPUutilities;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SingleEntity;

namespace Voxalia.ClientGame.EntitySystem
{
    public class ModelEntity: PhysicsEntity
    {
        public Model model;

        public Location scale = Location.One;

        public string mod;

        public Matrix4 transform;

        public Location Offset;

        public ModelCollisionMode mode = ModelCollisionMode.AABB;

        public BEPUutilities.Vector3 ModelMin;
        public BEPUutilities.Vector3 ModelMax;

        public ModelEntity(string model_in, Region tregion)
            : base(tregion, true, true)
        {
            mod = model_in;
        }

        public void TurnIntoPlane(PlayerEntity pilot) // TODO: Character!
        {
            PlanePilot = pilot;
            Plane = new PlaneMotionConstraint(this);
            TheRegion.PhysicsWorld.Add(Plane);
        }

        public float PlaneFastStrength
        {
            get
            {
                return GetMass() * 18f;
            }
        }

        public float PlaneRegularStrength
        {
            get
            {
                return GetMass() * 5f;
            }
        }

        public float PlaneSlowStrength
        {
            get
            {
                return GetMass() * 8f;
            }
        }

        public PlaneMotionConstraint Plane = null;

        public PlayerEntity PlanePilot = null; // TODO: Character!

        public class PlaneMotionConstraint : SingleEntityConstraint
        {
            ModelEntity Plane;
            
            public PlaneMotionConstraint(ModelEntity pln)
            {
                Plane = pln;
                Entity = pln.Body;
            }

            public override void ExclusiveUpdate()
            {
                if (Plane.PlanePilot == null)
                {
                    return; // Don't fly when there's nobody driving this!
                }
                // TODO: Special case for motion on land: only push forward if W key is pressed? Or maybe apply that rule in general?
                // Collect the plane's relative vectors
                BEPUutilities.Vector3 forward = BEPUutilities.Quaternion.Transform(BEPUutilities.Vector3.UnitY, Entity.Orientation);
                BEPUutilities.Vector3 side = BEPUutilities.Quaternion.Transform(BEPUutilities.Vector3.UnitX, Entity.Orientation);
                BEPUutilities.Vector3 up = BEPUutilities.Quaternion.Transform(BEPUutilities.Vector3.UnitZ, Entity.Orientation);
                // Engines!
                BEPUutilities.Vector3 force = forward * (Plane.PlaneRegularStrength + (Plane.PlanePilot.SprintOrWalk < 0 ? Plane.PlaneSlowStrength : Plane.PlaneFastStrength) * Plane.PlanePilot.SprintOrWalk) * Delta;
                entity.ApplyLinearImpulse(ref force);
                entity.ApplyImpulse(forward * 5 + entity.Position, up * Plane.PlanePilot.YMove * entity.Mass * 2.5f * Delta);
                entity.ApplyImpulse(side * 5 + entity.Position, up * -Plane.PlanePilot.XMove * entity.Mass * 3f * Delta);
                entity.ApplyImpulse(forward * 5 + entity.Position, side * ((Plane.PlanePilot.ItemRight ? 1 : 0) + (Plane.PlanePilot.ItemLeft ? -1 : 0)) * entity.Mass * 3f * Delta);
                // Apply air drag
                Entity.ModifyLinearDamping(0.2f); // TODO: arbitrary constant
                Entity.ModifyAngularDamping(0.7f); // TODO: arbitrary constant
                // Ensure we're active if flying!
                Entity.ActivityInformation.Activate();
            }

            public override double SolveIteration()
            {
                return 0; // Do nothing
            }

            double Delta;

            public override void Update(double dt)
            {
                Delta = dt;
            }
        }

        public void NoLongerAPlane() // TODO: Use me!
        {

        }

        public float LiftStrength
        {
            get
            {
                return GetMass() * 20f;
            }
        }

        public float FallStrength
        {
            get
            {
                return GetMass() * 9f;
            }
        }

        public void TurnIntoHelicopter(PlayerEntity pilot) // TODO: Character!
        {
            HeloPilot = pilot;
            Helo = new HelicopterMotionConstraint(this);
            TheRegion.PhysicsWorld.Add(Helo);
        }

        public HelicopterMotionConstraint Helo = null;

        public PlayerEntity HeloPilot = null; // TODO: Character!

        public float HeloTiltMod = 1f;

        public class HelicopterMotionConstraint : SingleEntityConstraint
        {
            ModelEntity Helicopter;

            public HelicopterMotionConstraint(ModelEntity heli)
            {
                Helicopter = heli;
                Entity = heli.Body;
            }

            public override void ExclusiveUpdate()
            {
                if (Helicopter.HeloPilot == null)
                {
                    return; // Don't fly when there's nobody driving this!
                }
                // Collect the helicopter's relative "up" vector
                BEPUutilities.Vector3 up = BEPUutilities.Quaternion.Transform(BEPUutilities.Vector3.UnitZ, Entity.Orientation);
                // Apply the amount of force necessary to counteract downward force, within a limit.
                // POTENTIAL: Adjust according to orientation?
                double uspeed = Math.Min(Helicopter.LiftStrength, -(Entity.LinearVelocity.Z + Entity.Space.ForceUpdater.Gravity.Z) * Entity.Mass);
                if (uspeed < 0f)
                {
                    uspeed += (uspeed - Helicopter.FallStrength) * Helicopter.HeloPilot.SprintOrWalk;
                }
                else
                {
                    uspeed += (Helicopter.LiftStrength - uspeed) * Helicopter.HeloPilot.SprintOrWalk;
                }
                BEPUutilities.Vector3 upvel = up * uspeed * Delta;
                Entity.ApplyLinearImpulse(ref upvel);
                // Rotate slightly to move in a direction.
                // At the same time, fight against existing rotation.
                BEPUutilities.Vector3 VecUp = new BEPUutilities.Vector3(Helicopter.HeloPilot.XMove * 0.2f * Helicopter.HeloTiltMod, Helicopter.HeloPilot.YMove * -0.2f * Helicopter.HeloTiltMod, 1);
                // TODO: Simplify yawrel calculation.
                float tyaw = (float)(Utilities.MatrixToAngles(Matrix.CreateFromQuaternion(Entity.Orientation)).Z * Utilities.PI180);
                BEPUutilities.Quaternion yawrel = BEPUutilities.Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.UnitZ, tyaw);
                VecUp = BEPUutilities.Quaternion.Transform(VecUp, yawrel);
                VecUp.Normalize();
                VecUp.Y = -VecUp.Y;
                BEPUutilities.Vector3 axis = BEPUutilities.Vector3.Cross(VecUp, up);
                double len = axis.Length();
                if (len > 0)
                {
                    axis /= len;
                    float angle = (float)Math.Asin(len);
                    if (!float.IsNaN(angle))
                    {
                        double avel = BEPUutilities.Vector3.Dot(Entity.AngularVelocity, axis);
                        BEPUutilities.Vector3 torque = axis * ((-angle) - 0.3f * avel);
                        torque *= Entity.Mass * Delta * 30;
                        Entity.ApplyAngularImpulse(ref torque);
                    }
                }
                // Spin in place
                float rotation = (Helicopter.HeloPilot.ItemRight ? -1f : 0f) + (Helicopter.HeloPilot.ItemLeft ? 1f : 0f);
                if (rotation * rotation > 0f)
                {
                    BEPUutilities.Vector3 rot = new BEPUutilities.Vector3(0, 0, rotation * 15f * Delta * Entity.Mass);
                    Entity.ApplyAngularImpulse(ref rot);
                }
                // Apply air drag
                Entity.ModifyLinearDamping(0.3f); // TODO: arbitrary constant
                Entity.ModifyAngularDamping(0.6f); // TODO: arbitrary constant
                // Ensure we're active if flying!
                Entity.ActivityInformation.Activate();
            }

            public override double SolveIteration()
            {
                return 0; // Do nothing
            }

            double Delta;

            public override void Update(double dt)
            {
                Delta = dt;
            }
        }

        public override void Tick()
        {
            if (Body == null)
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this); ?
                return;
            }
            base.Tick();
        }

        public void PreHandleSpawn()
        {
            model = TheClient.Models.GetModel(mod);
            model.LoadSkin(TheClient.Textures);
            int ignoreme;
            if (mode == ModelCollisionMode.PRECISE)
            {
                Shape = TheClient.Models.Handler.MeshToBepu(model.Original, out ignoreme);
            }
            else if (mode == ModelCollisionMode.CONVEXHULL)
            {
                Shape = TheClient.Models.Handler.MeshToBepuConvex(model.Original, out ignoreme);
            }
            else if (mode == ModelCollisionMode.AABB)
            {
                List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.Original);
                Location zero = new Location(vecs[0]);
                AABB abox = new AABB() { Min = zero, Max = zero };
                for (int v = 1; v < vecs.Count; v++)
                {
                    abox.Include(new Location(vecs[v]));
                }
                Location size = abox.Max - abox.Min;
                Location center = abox.Max - size / 2;
                Shape = new BoxShape((float)size.X * (float)scale.X, (float)size.Y * (float)scale.Y, (float)size.Z * (float)scale.Z);
                Offset = -center;
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.Original);
                // Location zero = new Location(vecs[0].X, vecs[0].Y, vecs[0].Z);
                double distSq = 0;
                for (int v = 1; v < vecs.Count; v++)
                {
                    if (vecs[v].LengthSquared() > distSq)
                    {
                        distSq = vecs[v].LengthSquared();
                    }
                }
                double size = Math.Sqrt(distSq);
                Offset = Location.Zero;
                Shape = new SphereShape((float)size * (float)scale.X);
            }
        }

        public override void SpawnBody()
        {
            PreHandleSpawn();
            base.SpawnBody();
            if (mode == ModelCollisionMode.PRECISE)
            {
                Offset = InternalOffset;
            }
            BEPUutilities.Vector3 offs = Offset.ToBVector();
            transform = Matrix4.CreateTranslation(ClientUtilities.Convert(Offset));
            List<BEPUutilities.Vector3> tvecs = TheClient.Models.Handler.GetVertices(model.Original);
            if (tvecs.Count == 0)
            {
                ModelMin = new BEPUutilities.Vector3(0, 0, 0);
                ModelMax = new BEPUutilities.Vector3(0, 0, 0);
            }
            else
            {
                ModelMin = tvecs[0];
                ModelMax = tvecs[0];
                foreach (BEPUutilities.Vector3 vec in tvecs)
                {
                    BEPUutilities.Vector3 tvec = vec + offs;
                    if (tvec.X < ModelMin.X) { ModelMin.X = tvec.X; }
                    if (tvec.Y < ModelMin.Y) { ModelMin.Y = tvec.Y; }
                    if (tvec.Z < ModelMin.Z) { ModelMin.Z = tvec.Z; }
                    if (tvec.X > ModelMax.X) { ModelMax.X = tvec.X; }
                    if (tvec.Y > ModelMax.Y) { ModelMax.Y = tvec.Y; }
                    if (tvec.Z > ModelMax.Z) { ModelMax.Z = tvec.Z; }
                }
            }
            if (GenBlockShadows)
            {
                BoxShape bs = new BoxShape(ModelMax.X - ModelMin.X, ModelMax.Y - ModelMin.Y, ModelMax.Z - ModelMin.Z);
                ShadowCastShape = bs.GetCollidableInstance();
                ShadowCastShape.LocalPosition = (ModelMax + ModelMin) * 0.5f + Body.Position;
                RigidTransform def = RigidTransform.Identity;
                ShadowCastShape.UpdateBoundingBoxForTransform(ref def);
            }
        }

        public void RenderSimpler()
        {
            if (!Visible || model.Meshes.Count == 0)
            {
                return;
            }
            TheClient.SetEnts();
            Matrix4 mat = GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            if (model.Meshes[0].vbo.Tex == null)
            {
                TheClient.Textures.White.Bind();
            }
            model.Draw(); // TODO: Animation?
        }

        public override void Render()
        {
            if (!Visible || model.Meshes.Count == 0)
            {
                return;
            }
            TheClient.SetEnts();
            RigidTransform rt = new RigidTransform(Body.Position, Body.Orientation);
            BEPUutilities.Vector3 bmin;
            BEPUutilities.Vector3 bmax;
            RigidTransform.Transform(ref ModelMin, ref rt, out bmin);
            RigidTransform.Transform(ref ModelMax, ref rt, out bmax);
            if (TheClient.MainWorldView.CFrust != null && !TheClient.MainWorldView.CFrust.ContainsBox(bmin, bmax))
            {
                return;
            }
            Matrix4 orient = GetOrientationMatrix();
            Matrix4 mat = transform * (Matrix4.CreateScale(ClientUtilities.Convert(scale)) * orient * Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition())));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            if (model.Meshes[0].vbo.Tex == null)
            {
                TheClient.Textures.White.Bind();
            }
            if (ShakesInWind)
            {
                model.ForceBoneNoOffset = true;
                OpenTK.Vector3 wind = ClientUtilities.Convert(TheRegion.ActualWind);
                float len = wind.Length;
                Matrix4 windtransf = Matrix4.CreateFromAxisAngle(wind / len, (float)Math.Min(len, 1.0));
                model.CustomAnimationAdjustments["b_bottom"] = windtransf;
                model.CustomAnimationAdjustments["b_top"] = windtransf;
            }
            model.Draw(0, null, 0, null, 0, null, ShakesInWind); // TODO: Animation(s)?
        }

        public bool ShakesInWind = false;
    }

    public class ModelEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] data)
        {
            ModelEntity me = new ModelEntity(tregion.TheClient.Network.Strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, PhysicsEntity.PhysicsNetworkDataLength, 4))), tregion);
            me.ApplyPhysicsNetworkData(data);
            byte moder = data[PhysicsEntity.PhysicsNetworkDataLength + 4];
            me.mode = (ModelCollisionMode)moder;
            me.scale = Location.FromDoubleBytes(data, PhysicsEntity.PhysicsNetworkDataLength + 4 + 1);
            me.ShakesInWind = data[PhysicsEntity.PhysicsNetworkDataLength + 4 + 1 + 24] == 1;
            return me;
        }
    }
}
