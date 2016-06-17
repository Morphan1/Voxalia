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

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: non-humanoid support
                return;
            }
            HumanoidEntity character = (HumanoidEntity)entity;
            float fireRate = FireRate * item.GetAttributeF("firerate_mod", 1f);
            if (item.Datum != 0 && !character.WaitingForClickRelease && (character.TheRegion.GlobalTickTime - character.LastGunShot >= fireRate))
            {
                float spread = Spread * item.GetAttributeF("spread_mod", 1f);
                float speed = Speed * item.GetAttributeF("speed_mod", 1f);
                int shots = (int)((float)Shots * item.GetAttributeF("shots_mod", 1f));
                for (int i = 0; i < shots; i++)
                {
                    BulletEntity be = new BulletEntity(character.TheRegion);
                    be.SetPosition(character.GetEyePosition()); // TODO: ItemPosition?
                    be.NoCollide.Add(character.EID);
                    Location ang = character.Direction;
                    ang.Yaw += Utilities.UtilRandom.NextDouble() * spread * 2 - spread;
                    ang.Pitch += Utilities.UtilRandom.NextDouble() * spread * 2 - spread;
                    be.SetVelocity(Utilities.ForwardVector_Deg(ang.Yaw, ang.Pitch) * speed);
                    be.Size = RoundSize;
                    be.Damage = ImpactDamage;
                    be.SplashSize = SplashSize;
                    be.SplashDamage = SplashMaxDamage;
                    character.TheRegion.SpawnEntity(be);
                }
                if (ShotPerClick)
                {
                    character.WaitingForClickRelease = true;
                }
                character.LastGunShot = character.TheRegion.GlobalTickTime;
                item.Datum -= 1;
                if (character is PlayerEntity)
                {
                    ((PlayerEntity)character).Network.SendPacket(new SetItemPacketOut(character.Items.Items.IndexOf(item), item));
                }
            }
            else if (item.Datum == 0 && !character.WaitingForClickRelease)
            {
                Reload(character, item);
            }
        }

        public bool Reload(HumanoidEntity character, ItemStack item)
        {
            if (character.Flags.HasFlag(YourStatusFlags.RELOADING))
            {
                return false;
            }
            int clipSize = (int)((float)ClipSize * item.GetAttributeF("clipsize_mod", 1f));
            if (item.Datum < clipSize)
            {
                for (int i = 0; i < character.Items.Items.Count; i++)
                {
                    ItemStack itemStack = character.Items.Items[i];
                    if (itemStack.Info is BulletItem && itemStack.SecondaryName == AmmoType)
                    {
                        if (itemStack.Count > 0)
                        {
                            int reloading = clipSize - item.Datum;
                            if (reloading > itemStack.Count)
                            {
                                reloading = itemStack.Count;
                            }
                            item.Datum += reloading;
                            if (character is PlayerEntity)
                            {
                                ((PlayerEntity)character).Network.SendPacket(new SetItemPacketOut(character.Items.Items.IndexOf(item), item));
                            }
                            itemStack.Count -= reloading;
                            if (itemStack.Count <= 0)
                            {
                                character.Items.RemoveItem(i + 1);
                            }
                            else
                            {
                                if (character is PlayerEntity)
                                {
                                    ((PlayerEntity)character).Network.SendPacket(new SetItemPacketOut(i, itemStack));
                                }
                            }
                        }
                        character.Flags |= YourStatusFlags.RELOADING;
                        character.WaitingForClickRelease = true;
                        character.LastGunShot = character.TheRegion.GlobalTickTime + ReloadDelay;
                        UpdatePlayer(character);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: non-humanoid support
                return;
            }
            HumanoidEntity character = (HumanoidEntity)entity;
            character.WaitingForClickRelease = false;
        }

        public override void ReleaseAltClick(Entity player, ItemStack item)
        {
        }

        public override void Use(Entity player, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: non-humanoid support
                return;
            }
            HumanoidEntity character = (HumanoidEntity)entity;
            character.WaitingForClickRelease = false;
            character.LastGunShot = 0;
            if (!(entity is PlayerEntity))
            {
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Flags &= ~YourStatusFlags.RELOADING;
            player.Flags &= ~YourStatusFlags.NEEDS_RELOAD;
            UpdatePlayer(player);
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: non-humanoid support
                return;
            }
            HumanoidEntity character = (HumanoidEntity)entity;
            if (character.Flags.HasFlag(YourStatusFlags.RELOADING) && (character.TheRegion.GlobalTickTime - character.LastGunShot >= FireRate))
            {
                character.Flags &= ~YourStatusFlags.RELOADING;
                UpdatePlayer(character);
            }
            else if (!character.Flags.HasFlag(YourStatusFlags.RELOADING) && (character.TheRegion.GlobalTickTime - character.LastGunShot < FireRate))
            {
                character.Flags |= YourStatusFlags.RELOADING;
                UpdatePlayer(character);
            }
            if (!character.Flags.HasFlag(YourStatusFlags.NEEDS_RELOAD) && item.Datum == 0)
            {
                character.Flags |= YourStatusFlags.NEEDS_RELOAD;
                UpdatePlayer(character);
            }
            else if (character.Flags.HasFlag(YourStatusFlags.NEEDS_RELOAD) && item.Datum != 0)
            {
                character.Flags &= ~YourStatusFlags.NEEDS_RELOAD;
                UpdatePlayer(character);
            }
        }

        public void UpdatePlayer(HumanoidEntity character)
        {
            // TODO: Should this be a method on PlayerEntity?
            if (character is PlayerEntity)
            {
                ((PlayerEntity)character).Network.SendPacket(new YourStatusPacketOut(character.GetHealth(), character.GetMaxHealth(), character.Flags));
            }
        }
    }
}
