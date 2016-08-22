using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using FreneticScript.TagHandlers.Objects;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class SmokeGrenadeEntity : GrenadeEntity, EntityUseable
    {
        // TODO: Possibly construct off of and save an itemstack rather than reconstructing it.
        System.Drawing.Color col;
        ParticleEffectNetType SmokeType;

        public SmokeGrenadeEntity(System.Drawing.Color _col, Region tregion, ParticleEffectNetType smokeType) :
            base(tregion)
        {
            col = _col;
            SmokeType = smokeType;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.SMOKE_GRENADE;
        }

        public override BsonDocument GetSaveData()
        {
            BsonDocument doc = new BsonDocument();
            AddPhysicsData(doc);
            doc["sg_color"] = col.ToArgb();
            doc["sg_smokeleft"] = SmokeLeft;
            doc["sg_type"] = SmokeType.ToString();
            return doc;
        }
        
        public int SmokeLeft;

        public double timer = 0;

        public double pulse = 1.0 / 15.0;

        public override void Tick()
        {
            timer += TheRegion.Delta;
            while (timer > pulse)
            {
                if (SmokeLeft <= 0)
                {
                    break;
                }
                Location colo = new Location(col.R / 255f, col.G / 255f, col.B / 255f);
                TheRegion.SendToAll(new ParticleEffectPacketOut(SmokeType, 5, GetPosition(), colo));
                timer -= pulse;
                SmokeLeft--;
            }
            base.Tick();
        }

        public void StartUse(Entity e)
        {
            if (Removed)
            {
                return;
            }
            if (e is HumanoidEntity)
            {
                ItemStack item;
                if (SmokeType == ParticleEffectNetType.SMOKE)
                {
                    item = TheServer.Items.GetItem("weapons/grenades/smoke");
                }
                else
                {
                    item = TheServer.Items.GetItem("weapons/grenades/smokesignal");
                    item.Attributes["big_smoke"] = new IntegerTag(1); // TODO: Insert into the smokesignal item itself! Or, at least, a boolean?
                }
                item.DrawColor = col;
                item.Attributes["max_smoke"] = new IntegerTag(SmokeLeft);
                ((HumanoidEntity)e).Items.GiveItem(item);
                RemoveMe();
            }
        }

        public void StopUse(Entity e)
        {
            // Do nothing
        }
    }

    public class SmokeGrenadeEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, BsonDocument doc)
        {
            ParticleEffectNetType efftype = (ParticleEffectNetType)Enum.Parse(typeof(ParticleEffectNetType), doc["sg_type"].AsString);
            SmokeGrenadeEntity grenade = new SmokeGrenadeEntity(System.Drawing.Color.FromArgb(doc["sg_color"].AsInt32), tregion, efftype);
            grenade.SmokeLeft = doc["sg_smokeleft"].AsInt32;
            grenade.ApplyPhysicsData(doc);
            return grenade;
        }
    }
}
