using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
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
            if (item.Datum != 0 && !player.WaitingForClickRelease && (player.TheServer.GlobalTickTime - player.LastGunShot >= FireRate))
            {
                for (int i = 0; i < Shots; i++)
                {
                    BulletEntity be = new BulletEntity(player.TheServer);
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
                    player.TheServer.SpawnEntity(be);
                }
                if (ShotPerClick)
                {
                    player.WaitingForClickRelease = true;
                }
                player.LastGunShot = player.TheServer.GlobalTickTime;
                item.Datum -= 1;
            }
            else if (item.Datum == 0 && !player.WaitingForClickRelease)
            {
                Reload(player, item);
                player.WaitingForClickRelease = true;
                player.LastGunShot = player.TheServer.GlobalTickTime + ReloadDelay;
            }
        }

        public bool Reload(PlayerEntity player, ItemStack item)
        {
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
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }
    }
}
