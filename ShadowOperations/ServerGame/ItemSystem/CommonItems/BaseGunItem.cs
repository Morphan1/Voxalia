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
        public BaseGunItem(string name, float round_size, float impact_damage, float splash_size, float splash_max_damage, float shot_speed, int clip_size, string ammo_type)
        {
            Name = name;
            RoundSize = round_size;
            ImpactDamage = impact_damage;
            SplashSize = splash_size;
            SplashMaxDamage = splash_max_damage;
            Speed = shot_speed;
            ClipSize = clip_size;
            AmmoType = ammo_type;
        }

        public float RoundSize;
        public float ImpactDamage;
        public float SplashSize;
        public float SplashMaxDamage;
        public float Speed;
        public int ClipSize;
        public string AmmoType;

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
            item.Datum = ClipSize; // TODO: Calculate how much ammo is had
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
            BulletEntity be = new BulletEntity(player.TheServer);
            be.SetPosition(player.GetEyePosition());
            be.NoCollide.Add(player.EID);
            be.SetVelocity(Utilities.ForwardVector_Deg(player.GetAngles().X, player.GetAngles().Y) * Speed);
            be.Size = RoundSize;
            be.Damage = ImpactDamage;
            be.SplashSize = SplashSize;
            be.SplashDamage = SplashMaxDamage;
            player.TheServer.SpawnEntity(be);
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }
    }
}
