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

namespace Voxalia.ServerGame.EntitySystem
{
    public class SmokegrenadeEntity : GrenadeEntity, EntityUseable
    {
        // TODO: Possibly construct off of and save an itemstack rather than reconstructing it.
        System.Drawing.Color col;
        ParticleEffectNetType SmokeType;

        public SmokegrenadeEntity(System.Drawing.Color _col, Region tregion, ParticleEffectNetType smokeType) :
            base(tregion)
        {
            col = _col;
            SmokeType = smokeType;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.SMOKE_GRENADE;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] bbytes = GetPhysicsBytes();
            byte[] res = new byte[bbytes.Length + 4 + 1 + 4];
            bbytes.CopyTo(res, 0);
            Utilities.IntToBytes(col.ToArgb()).CopyTo(res, bbytes.Length);
            res[bbytes.Length + 4] = (byte)SmokeType;
            Utilities.IntToBytes(SmokeLeft).CopyTo(res, bbytes.Length + 4 + 1);
            return res;
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

    public class SmokegrenadeEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int plen = 12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4 + 1;
            int colo = Utilities.BytesToInt(Utilities.BytesPartial(input, plen, 4));
            byte effecttype = input[plen + 4];
            int smokeleft = Utilities.BytesToInt(Utilities.BytesPartial(input, plen + 4 + 1, 4));
            SmokegrenadeEntity grenade = new SmokegrenadeEntity(System.Drawing.Color.FromArgb(colo), tregion, (ParticleEffectNetType)effecttype);
            grenade.SmokeLeft = smokeleft;
            grenade.ApplyBytes(input);
            return grenade;
        }
    }
}
