using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using ShadowOperations.ServerGame.ItemSystem;
using ShadowOperations.ServerGame.ItemSystem.CommonItems;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class PlayerEntity: EntityLiving
    {
        public Location HalfSize = new Location(0.5f, 0.5f, 0.9f);

        public Connection Network;

        public string Name;

        public string Host;

        public string Port;

        public string IP;

        public byte LastPingByte = 0;

        public bool Upward = false;
        public bool Downward = false;
        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;

        public bool Click = false;
        public bool AltClick = false;

        bool pkick = false;

        public bool FlashLightOn = false;

        public List<ItemStack> Items = new List<ItemStack>();

        public int cItem = 0;

        public SingleAnimation hAnim = null;
        public SingleAnimation tAnim = null;
        public SingleAnimation lAnim = null;

        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted</param>
        /// <returns>A valid item</returns>
        public ItemStack GetItemForSlot(int slot)
        {
            while (slot < 0)
            {
                slot += Items.Count + 1;
            }
            while (slot > Items.Count)
            {
                slot -= Items.Count + 1;
            }
            if (slot == 0)
            {
                return new ItemStack("Air", TheServer, 1, "clear", "Air", "An empty slot.", Color.White.ToArgb(), "blank.dae", true);
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public void Kick(string message)
        {
            if (pkick)
            {
                return;
            }
            pkick = true;
            Network.SendMessage("Kicking you: " + message);
            // TODO: Broadcast kick message
            SysConsole.Output(OutputType.INFO, "Kicking " + this + ": " + message);
            if (Network.Alive)
            {
                Network.PrimarySocket.Close(5);
            }
            TheServer.DespawnEntity(this);
        }

        public Location Direction;

        bool pup = false;

        public PhysicsEntity Grabbed = null;

        public float GrabForce = 0;

        public List<HookInfo> Hooks = new List<HookInfo>();

        public double ItemCooldown = 0;

        public PlayerEntity(Server tserver, Connection conn)
            : base(tserver, true, 100f)
        {
            Network = conn;
            SetMass(100);
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Shape.AngularDamping = 1;
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            GiveItem(new ItemStack("open_hand", TheServer, 1, "items/open_hand", "Open Hand", "Grab things!", Color.White.ToArgb(), "items/common/hand.dae", true));
            GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/9mm_pistol_gun", "9mm Pistol", "It shoots bullets!", Color.White.ToArgb(), "items/weapons/gun01.dae", false));
            GiveItem(new ItemStack("bow", TheServer, 1, "items/bow", "Bow", "It shoots arrows!", Color.White.ToArgb(), "items/weapons/bow.dae", false));
            GiveItem(new ItemStack("hook", TheServer, 1, "items/hook", "Grappling Hook", "Grab distant things!", Color.White.ToArgb(), "items/common/hook.dae", true));
            GiveItem(new ItemStack("flashlight", TheServer, 1, "items/flashlight", "Flashlight", "Lights things up!", Color.White.ToArgb(), "items/common/flashlight.dae", false));
            SetHealth(Health);
        }

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                if (hAnim != null && hAnim.Name == anim)
                {
                    return;
                }
                hAnim = TheServer.Animations.GetAnimation(anim);
            }
            else if (mode == 1)
            {
                if (tAnim != null && tAnim.Name == anim)
                {
                    return;
                }
                tAnim = TheServer.Animations.GetAnimation(anim);
            }
            else
            {
                if (lAnim != null && lAnim.Name == anim)
                {
                    return;
                }
                lAnim = TheServer.Animations.GetAnimation(anim);
            }
            TheServer.SendToAll(new AnimationPacketOut(this, anim, mode));
        }

        public void GiveItem(ItemStack item)
        {
            // TODO: stacking
            item.Info.PrepItem(this, item);
            Items.Add(item);
            Network.SendPacket(new SpawnItemPacketOut(Items.Count - 1, item));
        }

        public void RemoveItem(int item)
        {
            ItemStack its = GetItemForSlot(item);
            if (item == cItem)
            {
                its.Info.SwitchFrom(this, its);
            }
            while (item < 0)
            {
                item += Items.Count + 1;
            }
            while (item > Items.Count)
            {
                item -= Items.Count + 1;
            }
            Items.RemoveAt(item - 1);
            Network.SendPacket(new RemoveItemPacketOut(item - 1));
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == TheServer.Collision.Player)
            {
                return false;
            }
            return TheServer.Collision.ShouldCollide(entry);
        }

        public override void Tick()
        {
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
            CollisionResult crGround = TheServer.Collision.CuboidLineTrace(new Location(HalfSize.X - 0.1f, HalfSize.Y - 0.1f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis);
            if (Upward && !fly && !pup && crGround.Hit && GetVelocity().Z < 1f)
            {
                Vector3 imp = (Location.UnitZ * GetMass() * 7f).ToBVector();
                Body.ApplyLinearImpulse(ref imp);
                Body.ActivityInformation.Activate();
                imp = -imp;
                crGround.HitEnt.ApplyLinearImpulse(ref imp);
                crGround.HitEnt.ActivityInformation.Activate();
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
            bool Slow = false;
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.Yaw * Utilities.PI180, fly ? Direction.Pitch * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (pvel.LengthSquared() > 4 * MoveSpeed * MoveSpeed)
            {
                pvel = pvel.Normalize() * 2 * MoveSpeed;
            }
            pvel *= MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            if (!fly)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), new Vector3((float)pvel.X, (float)pvel.Y, 0) * (crGround.Hit ? 1f : 0.1f));
                Body.ActivityInformation.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            if (Hooks.Count > 0)
            {
                if (Downward)
                {
                    for (int i = 0; i < Hooks.Count; i++)
                    {
                        Hooks[i].JD.Max += (float)TheServer.Delta;
                        Hooks[i].JD.Min += (float)TheServer.Delta;
                        TheServer.DestroyJoint(Hooks[i].JD);
                        TheServer.AddJoint(Hooks[i].JD);
                    }
                }
                else if (Upward)
                {
                    for (int i = 0; i < Hooks.Count; i++)
                    {
                        if (Hooks[i].JD.Max > 1)
                        {
                            Hooks[i].JD.Max -= (float)TheServer.Delta;
                            Hooks[i].JD.Min -= (float)TheServer.Delta;
                        }
                        TheServer.DestroyJoint(Hooks[i].JD);
                        TheServer.AddJoint(Hooks[i].JD);
                    }
                }
            }
            if (Grabbed != null)
            {
                if (Grabbed.IsSpawned && (Grabbed.GetPosition() - GetEyePosition()).LengthSquared() < 5 * 5 + Grabbed.Widest * Grabbed.Widest)
                {
                    Location pos = GetEyePosition() + Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch) * (2 + Grabbed.Widest);
                    if (GrabForce >= Grabbed.GetMass())
                    {
                        Grabbed.Body.LinearVelocity = new Vector3(0, 0, 0);
                    }
                    Location tvec = (pos - Grabbed.GetPosition());
                    double len = tvec.Length();
                    if (len == 0)
                    {
                        len = 1;
                    }
                    Vector3 push = ((-Grabbed.GetVelocity()).Normalize() * GrabForce + (tvec / len) * GrabForce).ToBVector() * Grabbed.Body.InverseMass;
                    if (push.LengthSquared() > len * len)
                    {
                        push /= (float)(push.Length() / len) / 10f;
                    }
                    Grabbed.Body.LinearVelocity += push;
                    Grabbed.Body.ActivityInformation.Activate();
                }
                else
                {
                    Grabbed = null;
                }
            }
            /*if (!Utilities.IsCloseTo((float)base.GetAngles().Z, 0, 1))
            {
                base.SetAngles(new Location(0, 0, 0));
            }*/ // TODO: Does this need to be readded?
            PlayerUpdatePacketOut pupo = new PlayerUpdatePacketOut(this);
            for (int i = 0; i < TheServer.Players.Count; i++)
            {
                if (TheServer.Players[i] != this)
                {
                    TheServer.Players[i].Network.SendPacket(pupo);
                }
            }
            if (GetVelocity().LengthSquared() > 1)
            {
                SetAnimation("human/" + StanceName() +  "/walk_lowquality", 1);
                SetAnimation("human/" + StanceName() + "/walk_lowquality", 2);
            }
            else
            {
                SetAnimation("human/" + StanceName() + "/idle01", 1);
                SetAnimation("human/" + StanceName() + "/idle01", 2);
            }
            ItemStack cit = GetItemForSlot(cItem);
            if (Click)
            {
                cit.Info.Click(this, cit);
                LastClick = TheServer.GlobalTickTime;
            }
            if (AltClick)
            {
                cit.Info.AltClick(this, cit);
                LastAltClick = TheServer.GlobalTickTime;
            }
        }

        public double LastClick = 0;

        public double LastAltClick = 0;

        public float MoveSpeed = 10;

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public Location GetEyePosition()
        {
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
            }
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public Location GetCenter()
        {
            return base.GetPosition();
        }

        public override string ToString()
        {
            return Name;
        }

        public override Quaternion GetOrientation()
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Direction.Pitch)
                * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Direction.Yaw);
        }

        public override void SetOrientation(Quaternion rot)
        {
            Matrix trot = Matrix.CreateFromQuaternion(rot);
            Location ang = Utilities.MatrixToAngles(trot);
            Direction.Yaw = ang.Yaw;
            Direction.Pitch = ang.Pitch;
        }

        public override void SetHealth(float health)
        {
            base.SetHealth(health);
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth()));
        }

        public override void SetMaxHealth(float maxhealth)
        {
            base.SetMaxHealth(maxhealth);
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth()));
        }

        public override void Die()
        {
            SetHealth(MaxHealth);
            if (TheServer.SpawnPoints.Count == 0)
            {
                SysConsole.Output(OutputType.WARNING, "No spawn points... generating one!");
                TheServer.SpawnEntity(new SpawnPointEntity(TheServer));
            }
            SpawnPointEntity spe = null;
            for (int i = 0; i < 10; i++)
            {
                spe = TheServer.SpawnPoints[Utilities.UtilRandom.Next(TheServer.SpawnPoints.Count)];
                if (!TheServer.Collision.CuboidLineTrace(HalfSize, spe.GetPosition(), spe.GetPosition() + new Location(0, 0, 0.01f)).Hit)
                {
                    break;
                }
            }
            SetPosition(spe.GetPosition());
        }

        public PlayerStance Stance = PlayerStance.STAND;

        public string StanceName()
        {
            return Stance.ToString().ToLower();
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.CollisionInformation.CollisionRules.Group = TheServer.Collision.Player;
        }
    }

    public enum PlayerStance : byte
    {
        STAND = 0,
        CROUCH = 1,
        CRAWL = 2
    }
}
