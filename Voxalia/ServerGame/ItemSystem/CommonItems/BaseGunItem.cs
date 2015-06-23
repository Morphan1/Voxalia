using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public abstract class BaseGunItem: BaseItemInfo
    {
        public BaseGunItem(string name, float round_size, float impact_damage, float splash_size, float splash_max_damage,
            float shot_speed, int clip_size, string ammo_type, float spread, int shots, float fire_rate, float reload_delay, bool shot_per_click)
        {
            Name = name;
            RoundSize = round_size;
            ImpactDamage = impact_damage;
            SplashSize = splash_size;
            SplashMaxDamage = splash_max_damage;
            Speed = shot_speed;
            ClipSize = clip_size;
            AmmoType = ammo_type;
            Spread = spread;
            Shots = shots;
            FireRate = fire_rate;
            ReloadDelay = reload_delay;
            ShotPerClick = shot_per_click;
        }

        public float RoundSize;
        public float ImpactDamage;
        public float SplashSize;
        public float SplashMaxDamage;
        public float Speed;
        public int ClipSize;
        public string AmmoType;
        public float Spread;
        public int Shots;
        public float FireRate;
        public float ReloadDelay;
        public bool ShotPerClick;

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
            if (item.Datum != 0 && !player.WaitingForClickRelease && (player.TheWorld.GlobalTickTime - player.LastGunShot >= FireRate))
            {
                for (int i = 0; i < Shots; i++)
                {
                    BulletEntity be = new BulletEntity(player.TheWorld);
                    be.SetPosition(player.GetEyePosition());
                    be.NoCollide.Add(player.EID);
                    Location ang = player.Direction;
                    ang.Yaw += Utilities.UtilRandom.NextDouble() * Spread * 2 - Spread;
                    ang.Pitch += Utilities.UtilRandom.NextDouble() * Spread * 2 - Spread;
                    be.SetVelocity(Utilities.ForwardVector_Deg(ang.Yaw, ang.Pitch) * Speed);
                    be.Size = RoundSize;
                    be.Damage = ImpactDamage;
                    be.SplashSize = SplashSize;
                    be.SplashDamage = SplashMaxDamage;
                    player.TheWorld.SpawnEntity(be);
                }
                if (ShotPerClick)
                {
                    player.WaitingForClickRelease = true;
                }
                player.LastGunShot = player.TheWorld.GlobalTickTime;
                item.Datum -= 1;
                player.Network.SendPacket(new SetItemPacketOut(player.Items.IndexOf(item), item));
            }
            else if (item.Datum == 0 && !player.WaitingForClickRelease)
            {
                Reload(player, item);
            }
        }

        public bool Reload(PlayerEntity player, ItemStack item)
        {
            if (player.Flags.HasFlag(YourStatusFlags.RELOADING))
            {
                return false;
            }
            if (item.Datum < ClipSize)
            {
                for (int i = 0; i < player.Items.Count; i++)
                {
                    ItemStack itemStack = player.Items[i];
                    if (itemStack.Info is BulletItem && itemStack.SecondaryName == AmmoType)
                    {
                        if (itemStack.Count > 0)
                        {
                            int reloading = ClipSize - item.Datum;
                            if (reloading > itemStack.Count)
                            {
                                reloading = itemStack.Count;
                            }
                            item.Datum += reloading;
                            player.Network.SendPacket(new SetItemPacketOut(player.Items.IndexOf(item), item));
                            itemStack.Count -= reloading;
                            if (itemStack.Count <= 0)
                            {
                                player.RemoveItem(i + 1);
                            }
                            else
                            {
                                player.Network.SendPacket(new SetItemPacketOut(i, itemStack));
                            }
                        }
                        player.Flags |= YourStatusFlags.RELOADING;
                        player.WaitingForClickRelease = true;
                        player.LastGunShot = player.TheWorld.GlobalTickTime + ReloadDelay;
                        return true;
                    }
                }
            }
            return false;
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
        {
            player.WaitingForClickRelease = false;
        }

        public override void Use(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
            player.WaitingForClickRelease = false;
            player.LastGunShot = 0;
            player.Flags &= ~YourStatusFlags.RELOADING;
            player.Flags &= ~YourStatusFlags.NEEDS_RELOAD;
            UpdatePlayer(player);
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }

        public override void Tick(PlayerEntity player, ItemStack item)
        {
            if (player.Flags.HasFlag(YourStatusFlags.RELOADING) && (player.TheWorld.GlobalTickTime - player.LastGunShot >= FireRate))
            {
                player.Flags &= ~YourStatusFlags.RELOADING;
                UpdatePlayer(player);
            }
            else if (!player.Flags.HasFlag(YourStatusFlags.RELOADING) && (player.TheWorld.GlobalTickTime - player.LastGunShot < FireRate))
            {
                player.Flags |= YourStatusFlags.RELOADING;
                UpdatePlayer(player);
            }
            if (!player.Flags.HasFlag(YourStatusFlags.NEEDS_RELOAD) && item.Datum == 0)
            {
                player.Flags |= YourStatusFlags.NEEDS_RELOAD;
                UpdatePlayer(player);
            }
            else if (player.Flags.HasFlag(YourStatusFlags.NEEDS_RELOAD) && item.Datum != 0)
            {
                player.Flags &= ~YourStatusFlags.NEEDS_RELOAD;
                UpdatePlayer(player);
            }
        }

        public void UpdatePlayer(PlayerEntity player)
        {
            player.Network.SendPacket(new YourStatusPacketOut(player.GetHealth(), player.GetMaxHealth(), player.Flags));
        }
    }
}
