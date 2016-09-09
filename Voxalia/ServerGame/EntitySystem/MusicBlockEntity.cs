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
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    class MusicBlockEntity : ModelEntity, EntityUseable, EntityDamageable
    {
        public ItemStack Original;

        // TODO: Heal with time?

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
        }

        public override EntityType GetEntityType()
        {
            return EntityType.MUSIC_BLOCK;
        }

        public override BsonDocument GetSaveData()
        {
            BsonDocument doc = new BsonDocument();
            AddPhysicsData(doc);
            doc["mb_item"] = Original.ServerBytes();
            doc["mb_health"] = (double)GetHealth();
            doc["mb_maxhealth"] = (double)GetMaxHealth();
            return doc;
        }
        
        public void StartUse(Entity user)
        {
            if (!Removed)
            {
                int itemMusicType = Original.GetAttributeI("music_type", 0);
                double itemMusicVolume = Original.GetAttributeF("music_volume", 0.5f);
                double itemMusicPitch = Original.GetAttributeF("music_pitch", 1f);
                TheRegion.PlaySound("sfx/musicnotes/" + itemMusicType, GetPosition(), itemMusicVolume, itemMusicPitch);
            }
        }

        public void StopUse(Entity user)
        {
            // Do nothing
        }

        public double Health = 5;

        public double MaxHealth = 5;

        public double GetHealth()
        {
            return Health;
        }

        public double GetMaxHealth()
        {
            return MaxHealth;
        }

        public void SetHealth(double health)
        {
            Health = health;
            if (health < 0)
            {
                RemoveMe();
                // TODO: Break into a grabbable item?
            }
        }

        public void SetMaxHealth(double health)
        {
            MaxHealth = health;
            if (Health > MaxHealth)
            {
                SetHealth(MaxHealth);
            }
        }

        public void Damage(double amount)
        {
            SetHealth(GetHealth() - amount);
        }
    }

    public class MusicBlockEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, BsonDocument doc)
        {
            ItemStack it = new ItemStack(doc["mb_item"].AsBinary, tregion.TheServer);
            MusicBlockEntity mbe = new MusicBlockEntity(tregion, it, Location.Zero);
            mbe.SetMaxHealth((double)doc["mb_maxhealth"].AsDouble);
            mbe.SetHealth((double)doc["mb_health"].AsDouble);
            return mbe;
        }
    }
}
