using Voxalia.Shared;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using BEPUphysics.Character;
using OpenTK.Graphics.OpenGL4;

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

        public Location ServerLocation = new Location(0, 0, 0);

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
        
        public PlayerEntity(Region tregion)
            : base(tregion, true, true)
        {
            SetMass(tmass / 2f);
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity.Tag == this)
            {
                return false;
            }
            return TheRegion.Collision.ShouldCollide(entry);
        }

        public bool IgnorePlayers(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == CollisionUtil.Player)
            {
                return false;
            }
            return TheRegion.Collision.ShouldCollide(entry);
        }
        
        public override void Tick()
        {
            if (CBody == null || Body == null)
            {
                return;
            }
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
            CBody.ViewDirection = Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch).ToBVector();
            bool fly = false;
            if (Upward && !fly && !pup && CBody.SupportFinder.HasSupport)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            // TODO: Adjust movement speed as needed
            /*
            if (Click)
            {
                ItemStack item = TheClient.GetItemForSlot(TheClient.QuickBarPos);
                if (item.SharedAttributes.ContainsKey("charge") && item.SharedAttributes["charge"] == 1f)
                {
                    float speed = item.SharedAttributes["cspeedm"];
                    intent_vel *= speed;
                }
            }
            */
            Vector2 movement = new Vector2(0, 0);
            if (Leftward)
            {
                movement.X = -1;
            }
            if (Rightward)
            {
                movement.X = 1;
            }
            if (Backward)
            {
                movement.Y = -1;
            }
            if (Forward)
            {
                movement.Y = 1;
            }
            if (movement.LengthSquared() > 0)
            {
                movement.Normalize();
            }
            CBody.HorizontalMotionConstraint.MovementDirection = movement;
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
            //base.Tick();
            //base.SetOrientation(Quaternion.Identity);
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

        public CharacterController CBody;
        
        public override void SpawnBody()
        {
            if (CBody != null)
            {
                DestroyBody();
            }
            // TODO: Better variable control! (Server should command every detail!)
            CBody = new CharacterController(ServerLocation.ToBVector(), (float)HalfSize.Z * 2f, (float)HalfSize.Z * 1.1f,
                (float)HalfSize.X, 0.01f, 10f, 1.0f, 1.3f, 5f, 2.5f, 1000f, 5f, 50f, 0.5f, 250f, 10f, 5f, 5000f);
            CBody.StanceManager.DesiredStance = Stance.Standing;
            CBody.ViewDirection = new Vector3(1f, 0f, 0f);
            CBody.Down = new Vector3(0f, 0f, -1f);
            CBody.Tag = this;
            Body = CBody.Body;
            Body.Tag = this;
            Body.AngularDamping = 1.0f;
            Shape = CBody.Body.CollisionInformation.Shape;
            ConvexEntityShape = CBody.Body.CollisionInformation.Shape;
            Body.CollisionInformation.CollisionRules.Group = CollisionUtil.Player;
            CBody.StepManager.MaximumStepHeight = 0.05f;
            CBody.StepManager.MinimumDownStepHeight = 0.05f;
            TheRegion.PhysicsWorld.Add(CBody);
        }

        public override void DestroyBody()
        {
            if (CBody == null)
            {
                return;
            }
            TheRegion.PhysicsWorld.Remove(CBody);
            CBody = null;
            Body = null;
        }

        public SpotLight Flashlight = null;

        public float MoveSpeed = 960;
        public float MoveRateCap = 1920;

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8: 1.1));
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
        }

        public override void SetVelocity(Location vel)
        {
            base.SetVelocity(vel);
        }

        public float Health;

        public float MaxHealth;

        public static OpenTK.Matrix4 PlayerAngleMat = OpenTK.Matrix4.CreateRotationZ((float)(270 * Utilities.PI180));

        public double aHTime;
        public double aTTime;
        public double aLTime;

        public override void Render()
        {
            if (TheClient.CVars.n_debugmovement.ValueB)
            {
                TheClient.Rendering.RenderLine(ServerLocation, GetPosition());
                TheClient.Rendering.RenderLineBox(ServerLocation + new Location(-0.2), ServerLocation + new Location(0.2));
            }
            if (TheClient.RenderingShadows || !TheClient.CVars.g_firstperson.ValueB)
            {
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
