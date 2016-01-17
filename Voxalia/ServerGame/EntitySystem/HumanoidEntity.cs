using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using BEPUphysics.Character;
using BEPUutilities;
using Voxalia.ServerGame.ItemSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class HumanoidEntity: CharacterEntity
    {
        public HumanoidEntity(Region tregion)
            : base(tregion, 100)
        {
        }

        public YourStatusFlags Flags = YourStatusFlags.NONE;

        public EntityInventory Items;

        public ModelEntity CursorMarker = null;

        public JointBallSocket GrabJoint = null;

        public List<HookInfo> Hooks = new List<HookInfo>();

        public double ItemCooldown = 0;

        public double ItemStartClickTime = -1;

        public bool WaitingForClickRelease = false;

        public bool FlashLightOn = false;

        /// <summary>
        /// The animation of the character's head.
        /// </summary>
        public SingleAnimation hAnim = null;

        /// <summary>
        /// The animation of the character's torso.
        /// </summary>
        public SingleAnimation tAnim = null;

        /// <summary>
        /// The animation of the character's legs.
        /// </summary>
        public SingleAnimation lAnim = null;

        public override void SpawnBody()
        {
            base.SpawnBody();
            if (CursorMarker == null)
            {
                CursorMarker = new ModelEntity("cube", TheRegion);
                CursorMarker.scale = new Location(0.1f, 0.1f, 0.1f);
                CursorMarker.mode = ModelCollisionMode.AABB;
                CursorMarker.CGroup = CollisionUtil.NonSolid;
                CursorMarker.Visible = false;
                CursorMarker.CanSave = false;
                TheRegion.SpawnEntity(CursorMarker);
            }
        }

        public override void DestroyBody()
        {
            if (CBody == null)
            {
                return;
            }
            base.DestroyBody();
            if (CursorMarker.IsSpawned && !CursorMarker.Removed)
            {
                CursorMarker.RemoveMe();
                CursorMarker = null;
            }
        }

        public override void Tick()
        {
            base.Tick();
            CursorMarker.SetPosition(GetEyePosition() + ForwardVector() * 0.9f);
            CursorMarker.SetOrientation(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(Direction.Pitch * Utilities.PI180)) *
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(Direction.Yaw * Utilities.PI180)));
        }

        public double LastClick = 0;

        public double LastGunShot = 0;

        public double LastBlockBreak = 0;

        public double LastBlockPlace = 0;

        public bool WasClicking = false;

        public double LastAltClick = 0;

        public bool WasAltClicking = false;

        public Location BlockBreakTarget;

        public double BlockBreakStarted = 0;

        public override Location GetEyePosition()
        {
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("special06.r");
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>();
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)(Direction.Pitch / 1.75f * Utilities.PI180)));
                adjs["spine05"] = rotforw;
                Matrix m4 = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 270) * Utilities.PI180) % 360f))
                    * head.GetBoneTotalMatrix(0, adjs) * (rotforw * Matrix.CreateTranslation(new Vector3(0, 0, 0.2f)));
                m4.Transpose();
                return GetPosition() + new Location(m4.Translation) * 1.5f;// TODO: Match clientside ray trace?
            }
            else
            {
                return GetPosition() + new Location(0, 0, CBHHeight * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8 : 1.5));
            }
        }

    }
}
