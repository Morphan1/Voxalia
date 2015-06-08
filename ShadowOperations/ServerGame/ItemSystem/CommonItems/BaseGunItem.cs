using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public abstract class BaseGunItem: BaseItemInfo
    {
        public BaseGunItem(string name, float round_size, float impact_damage, float splash_size, float splash_max_damage, float shot_speed, int clip_size, string ammo_type, float spread, int shots, float fire_rate)
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

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
            item.Datum = ClipSize;
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
            if (item.Datum != 0 && !player.WaitingForClickRelease && (FireRate == -1 || player.TheServer.GlobalTickTime - player.LastGunShot >= FireRate))
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
                    item.Datum -= 1;
                    if (item.Datum == 0)
                    {
                        break;
                    }
                }
                if (FireRate == -1)
                {
                    player.WaitingForClickRelease = true;
                }
                player.LastGunShot = player.TheServer.GlobalTickTime;
            }
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
