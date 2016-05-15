using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.WorldSystem;
using FreneticScript.TagHandlers.Objects;
using BEPUutilities;

namespace Voxalia.ServerGame.EntitySystem
{
    class MusicBlockEntity : ModelEntity, EntityUseable, EntityDamageable
    {
        public ItemStack Original;

        public MusicBlockEntity(Region tregion, ItemStack orig, Location pos)
            : base("mapobjects/customblocks/musicblock", tregion)
        {
            Original = orig;
            SetMass(0);
            SetPosition(pos.GetBlockLocation() + new Location(0.5));
            SetOrientation(Quaternion.Identity);
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            //SetPosition(GetPosition() + offset);
            //ForceNetwork();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.MUSIC_BLOCK;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] itdat = Original.ServerBytes();
            byte[] n = new byte[itdat.Length + 4 + 12 + 4 + 4];
            Utilities.IntToBytes(itdat.Length).CopyTo(n, 0);
            itdat.CopyTo(n, 4);
            GetPosition().ToBytes().CopyTo(n, 4 + itdat.Length);
            Utilities.FloatToBytes(GetMaxHealth()).CopyTo(n, 4 + itdat.Length + 12);
            Utilities.FloatToBytes(GetHealth()).CopyTo(n, 4 + itdat.Length + 12 + 4);
            return n;
        }

        public void StartUse(Entity user)
        {
            if (!Removed)
            {
                int itemMusicType = Original.GetAttributeI("music_type", 0);
                float itemMusicVolume = Original.GetAttributeF("music_volume", 0.5f);
                float itemMusicPitch = Original.GetAttributeF("music_pitch", 1f);
                TheRegion.PlaySound("sfx/musicnotes/" + itemMusicType, GetPosition(), itemMusicVolume, itemMusicPitch);
            }
        }

        public void StopUse(Entity user)
        {
            // Do nothing
        }

        public float Health = 5;

        public float MaxHealth = 5;

        public float GetHealth()
        {
            return Health;
        }

        public float GetMaxHealth()
        {
            return MaxHealth;
        }

        public void SetHealth(float health)
        {
            Health = health;
            if (health < 0)
            {
                RemoveMe();
            }
        }

        public void SetMaxHealth(float health)
        {
            MaxHealth = health;
            if (Health > MaxHealth)
            {
                SetHealth(MaxHealth);
            }
        }

        public void Damage(float amount)
        {
            SetHealth(GetHealth() - amount);
        }
    }

    public class MusicBlockEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int len = Utilities.BytesToInt(Utilities.BytesPartial(input, 0, 4));
            byte[] itm = new byte[len];
            Array.Copy(input, 4, itm, 0, len);
            ItemStack it = new ItemStack(itm, tregion.TheServer);
            Location pos = Location.FromBytes(input, 4 + len);
            MusicBlockEntity mbe = new MusicBlockEntity(tregion, it, pos);
            mbe.SetMaxHealth(Utilities.BytesToFloat(Utilities.BytesPartial(input, 4 + len + 12, 4)));
            mbe.SetHealth(Utilities.BytesToFloat(Utilities.BytesPartial(input, 4 + len + 12 + 4, 4)));
            return mbe;
        }
    }
}
