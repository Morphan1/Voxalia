using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.NetworkSystem.PacketsIn;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public class PlayerEntity: PhysicsEntity, EntityAnimated
    {
        public YourStatusFlags ServerFlags = YourStatusFlags.NONE;

        public SingleAnimation hAnim;
        public SingleAnimation tAnim;
        public SingleAnimation lAnim;

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                hAnim = TheClient.Animations.GetAnimation(anim);
                aHTime = 0;
            }
            else if (mode == 1)
            {
                tAnim = TheClient.Animations.GetAnimation(anim);
                aTTime = 0;
            }
            else
            {
                lAnim = TheClient.Animations.GetAnimation(anim);
                aLTime = 0;
            }
        }

        public Location HalfSize = new Location(0.55f, 0.55f, 1.3f);

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Click = false;
        public bool AltClick = false;
        public bool Walk = false;

        public float tmass = 100;

        bool pup = false;

        public Model model;

        public ConvexShape WheelShape = null;

        public PlayerEntity(World tworld)
            : base(tworld, true, true)
        {
            SetMass(tmass / 2f);
            Shape = new BoxShape((float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            WheelShape = new SphereShape((float)HalfSize.X);
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004.dae");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity.Tag == this)
            {
                return false;
            }
            return TheWorld.Collision.ShouldCollide(entry);
        }

        public bool IgnorePlayers(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == CollisionUtil.Player)
            {
                return false;
            }
            return TheWorld.Collision.ShouldCollide(entry);
        }

        public PlayerStance Stance = PlayerStance.STAND;

        public override void Tick()
        {
            Direction.Yaw += MouseHandler.MouseDelta.X;
            Direction.Pitch += MouseHandler.MouseDelta.Y;
            while (Direction.Yaw < 0)
            {
                Direction.Yaw += 360;
            }
            while (Direction.Yaw > 360)
            {
                Direction.Yaw -= 360;
            }
            if (Direction.Pitch > 89.9f)
            {
                Direction.Pitch = 89.9f;
            }
            if (Direction.Pitch < -89.9f)
            {
                Direction.Pitch = -89.9f;
            }
            bool fly = false;
            CollisionResult crGround = TheWorld.Collision.CuboidLineTrace(new Location(HalfSize.X - 0.01f, HalfSize.Y - 0.01f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis);
            if (Upward && !fly && !pup && crGround.Hit && GetVelocity().Z < 1f)
            {
                Vector3 imp = (Location.UnitZ * GetMass() * 7f).ToBVector();
                Body.ApplyLinearImpulse(ref imp);
                Body.ActivityInformation.Activate();
                imp = -imp;
                if (crGround.HitEnt != null)
                {
                    crGround.HitEnt.ApplyLinearImpulse(ref imp);
                    crGround.HitEnt.ActivityInformation.Activate();
                }
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            Location movement = new Location(0, 0, 0);
            if (Leftward)
            {
                movement.Y = -1;
            }
            if (Rightward)
            {
                movement.Y = 1;
            }
            if (Backward)
            {
                movement.X = 1;
            }
            if (Forward)
            {
                movement.X = -1;
            }
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.Yaw * Utilities.PI180, fly ? Direction.Pitch * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * MoveSpeed * (Walk ? 0.7f : 1f);
            if (Stance == PlayerStance.CROUCH)
            {
                intent_vel *= 0.5f;
            }
            else if (Stance == PlayerStance.CRAWL)
            {
                intent_vel *= 0.3f;
            }
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (pvel.LengthSquared() > MoveRateCap * MoveRateCap)
            {
                pvel = pvel.Normalize() * MoveRateCap;
            }
            pvel *= TheWorld.Delta * (crGround.Hit ? 1f : 0.1f);
            if (!fly)
            {
                Vector3 move = new Vector3((float)pvel.X, (float)pvel.Y, 0);
                Body.ApplyLinearImpulse(ref move);
                Body.ActivityInformation.Activate();
            }
            else
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            KeysPacketData kpd = (Forward ? KeysPacketData.FORWARD : 0) | (Backward ? KeysPacketData.BACKWARD : 0)
                 | (Leftward ? KeysPacketData.LEFTWARD : 0) | (Rightward ? KeysPacketData.RIGHTWARD : 0)
                 | (Upward ? KeysPacketData.UPWARD : 0) | (Walk ? KeysPacketData.WALK: 0)
                  | (Click ? KeysPacketData.CLICK : 0) | (AltClick ? KeysPacketData.ALTCLICK: 0);
            TheClient.Network.SendPacket(new KeysPacketOut(kpd, Direction));
            if (Flashlight != null)
            {
                Flashlight.Direction = Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
                Flashlight.Reposition(GetEyePosition() + Utilities.ForwardVector_Deg(Direction.Yaw, 0) * 0.3f);
            }
            base.Tick();
            base.SetOrientation(Quaternion.Identity);
            aHTime += TheClient.Delta;
            aTTime += TheClient.Delta;
            aLTime += TheClient.Delta;
            if (hAnim != null)
            {
                if (aHTime >= hAnim.Length)
                {
                    aHTime = 0;
                }
            }
            if (tAnim != null)
            {
                if (aTTime >= tAnim.Length)
                {
                    aTTime = 0;
                }
            }
            if (lAnim != null)
            {
                if (aLTime >= lAnim.Length)
                {
                    aLTime = 0;
                }
            }
        }

        public BEPUphysics.Entities.Entity WheelBody;

        public BEPUphysics.Constraints.TwoEntity.Joints.BallSocketJoint bsj;

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.LinearDamping = 0;
            Body.AngularDamping = 1;
            WheelBody = new BEPUphysics.Entities.Entity(WheelShape, tmass / 2f);
            WheelBody.Orientation = Quaternion.Identity;
            WheelBody.Position = Body.Position + new Vector3(0, 0, -(float)HalfSize.Z);
            WheelBody.CollisionInformation.CollisionRules.Specific.Add(Body.CollisionInformation.CollisionRules, BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase);
            Body.CollisionInformation.CollisionRules.Specific.Add(WheelBody.CollisionInformation.CollisionRules, BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase);
            WheelBody.Tag = this;
            WheelBody.AngularDamping = 0.5f;
            WheelBody.LinearDamping = 0.5f;
            TheWorld.PhysicsWorld.Add(WheelBody);
            bsj = new BEPUphysics.Constraints.TwoEntity.Joints.BallSocketJoint(Body, WheelBody, WheelBody.Position);
            TheWorld.PhysicsWorld.Add(bsj);
        }

        public override void DestroyBody()
        {
            if (bsj != null)
            {
                TheWorld.PhysicsWorld.Remove(bsj);
                bsj = null;
            }
            base.DestroyBody();
            if (WheelBody != null)
            {
                TheWorld.PhysicsWorld.Remove(WheelBody);
                WheelBody = null;
            }
        }

        public SpotLight Flashlight = null;

        public float MoveSpeed = 480;
        public float MoveRateCap = 960;

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8);
            /*
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("head");
                Matrix m4 = head.GetBoneTotalMatrix(0);
                m4.Transpose();
                return GetPosition() + Location.FromBVector(m4.Translation);
            }
            else
            {
                return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8f);
            }*/
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z + HalfSize.X);
        }

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z + HalfSize.X));
            if (WheelBody != null)
            {
                WheelBody.Position = pos.ToBVector() + new Vector3(0, 0, (float)HalfSize.X);
            }
        }

        public override void SetVelocity(Location vel)
        {
            base.SetVelocity(vel);
            if (WheelBody != null)
            {
                WheelBody.LinearVelocity = vel.ToBVector();
            }
        }

        public float Health;

        public float MaxHealth;

        public static OpenTK.Matrix4 PlayerAngleMat = OpenTK.Matrix4.CreateRotationZ((float)(270 * Utilities.PI180));

        public double aHTime;
        public double aTTime;
        public double aLTime;

        public override void Render()
        {
            if (TheClient.RenderingShadows || !TheClient.CVars.g_firstperson.ValueB)
            {
                // TODO: debug mode ... match this on all living entities
                /*
                OpenTK.Matrix4 bodymat = GetTransformationMatrix();
                Location loc = new Location(bodymat.ExtractTranslation());
                bodymat = OpenTK.Matrix4.CreateFromQuaternion(bodymat.ExtractRotation());
                TheClient.Rendering.RenderLineBox(loc - HalfSize, loc + HalfSize, bodymat);
                */
                OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateScale(1.5f)
                    * OpenTK.Matrix4.CreateRotationZ((float)(Direction.Yaw * Utilities.PI180))
                    * PlayerAngleMat
                    * OpenTK.Matrix4.CreateTranslation(GetPosition().ToOVector());
                GL.UniformMatrix4(2, false, ref mat);
                TheClient.Rendering.SetMinimumLight(0.0f);
                model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            }
        }
    }
}
