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
        public Location HalfSize = new Location(0.5f, 0.5f, 1f);

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

        public List<ItemStack> Items = new List<ItemStack>();

        public int cItem = 0;

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
                return new ItemStack("Air", TheServer, 1, "clear", "Air", "An empty slot.", Color.White.ToArgb());
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

        public PlayerEntity(Server tserver, Connection conn)
            : base(tserver, true, 100f)
        {
            Network = conn;
            SetMass(100);
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Shape.AngularDamping = 1;
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            GiveItem(new ItemStack("open_hand", TheServer, 1, "items/open_hand", "Open Hand", "Grab things!", Color.White.ToArgb()));
            GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/9mm_pistol_gun", "9mm Pistol", "It shoots bullets!", Color.White.ToArgb()));
            GiveItem(new ItemStack("hook", TheServer, 1, "items/hook", "Grappling Hook", "Grab distant things!", Color.White.ToArgb()));
            SetHealth(Health);
        }

        public void GiveItem(ItemStack item)
        {
            // TODO: stacking
            item.Info.PrepItem(this, item);
            Items.Add(item);
            Network.SendPacket(new SpawnItemPacketOut(Items.Count - 1, item));
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            bool isThis = ((EntityCollidable)entry).Entity.Tag == this;
            if (isThis)
            {
                return false;
            }
            return entry.CollisionRules.Group != TheServer.Collision.NonSolid;
        }

        public override void Tick()
        {
            while (Direction.X < 0)
            {
                Direction.X += 360;
            }
            while (Direction.X > 360)
            {
                Direction.X -= 360;
            }
            if (Direction.Y > 89.9f)
            {
                Direction.Y = 89.9f;
            }
            if (Direction.Y < -89.9f)
            {
                Direction.Y = -89.9f;
            }
            bool fly = false;
            CollisionResult crGround = TheServer.Collision.CuboidLineTrace(new Location(HalfSize.X, HalfSize.Y, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis);
            if (Upward && !fly && !pup && crGround.Hit && GetVelocity().Z < 1f)
            {
                Vector3 imp = (Location.UnitZ * GetMass() * 5f).ToBVector();
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
                movement = Utilities.RotateVector(movement, Direction.X * Utilities.PI180, fly ? Direction.Y * Utilities.PI180 : 0).Normalize();
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
                        Location norm = -(Hooks[i].One.GetPosition() - GetCenter()).Normalize();
                        Vector3 vel = (norm * GetMass() * 1.2f).ToBVector();
                        Body.ApplyLinearImpulse(ref vel);
                        if (Hooks[i].Hit.GetMass() > 0)
                        {
                            vel = -vel;
                            Hooks[i].Hit.Body.ApplyLinearImpulse(ref vel);
                            Hooks[i].Hit.Body.ActivityInformation.Activate();
                        }
                        TheServer.DestroyJoint(Hooks[i].JD);
                        TheServer.AddJoint(Hooks[i].JD);
                    }
                }
                else if (Upward)
                {
                    for (int i = 0; i < Hooks.Count; i++)
                    {
                        Hooks[i].JD.Max -= (float)TheServer.Delta;
                        Hooks[i].JD.Min -= (float)TheServer.Delta;
                        if (Hooks[i].JD.Max < 1)
                        {
                            Hooks[i].JD.Min += 1 - Hooks[i].JD.Max;
                            Hooks[i].JD.Max = 1;
                        }
                        Location norm = (Hooks[i].One.GetPosition() - GetCenter()).Normalize();
                        Vector3 vel = (norm * GetMass() * 1.2f).ToBVector();
                        Body.ApplyLinearImpulse(ref vel);
                        if (Hooks[i].Hit.GetMass() > 0)
                        {
                            vel = -vel;
                            Hooks[i].Hit.Body.ApplyLinearImpulse(ref vel);
                            Hooks[i].Hit.Body.ActivityInformation.Activate();
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
                    Location pos = GetEyePosition() + Utilities.ForwardVector_Deg(Direction.X, Direction.Y) * (2 + Grabbed.Widest);
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
            PlayerUpdatePacketOut pupo = new PlayerUpdatePacketOut(this);
            for (int i = 0; i < TheServer.Players.Count; i++)
            {
                if (TheServer.Players[i] != this)
                {
                    TheServer.Players[i].Network.SendPacket(pupo);
                }
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

        public override Location GetAngles()
        {
            return Direction;
        }

        public override void SetAngles(Location rot)
        {
            Direction = rot;
        }

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * 1.6f);
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
    }
}
