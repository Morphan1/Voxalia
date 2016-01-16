using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    class TargetEntity: CharacterEntity
    {
        public TargetEntity(Region tregion) :
            base (tregion, 100)
        {
            SetMass(70);
            Items = new EntityInventory(tregion, this);
            // TODO: Better way to gather item details!
            Items.GiveItem(TheServer.Items.GetItem("weapons/rifles/m4"));
            Items.GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 1000, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", System.Drawing.Color.White, "items/weapons/ammo/rifle_round", false));
            Items.cItem = 1;
            Items.Items[0].Info.PrepItem(this, Items.Items[0]);
        }
        
        public override EntityType GetEntityType()
        {
            return EntityType.TARGET_ENTITY;
        }

        public double NextBoing = 0;

        public double NextAttack = 0;

        public override byte[] GetSaveBytes()
        {
            return null; // TODO: Save/load
        }

        public override void Tick()
        {
            base.Tick();
            NextBoing -= TheRegion.Delta;
            if (NextBoing <= 0)
            {
                NextBoing = Utilities.UtilRandom.NextDouble() * 2;
                XMove = (float)Utilities.UtilRandom.NextDouble() * 2f - 1f;
                YMove = (float)Utilities.UtilRandom.NextDouble() * 2f - 1f;
                Upward = Utilities.UtilRandom.Next(100) > 75;
            }
            NextAttack -= TheRegion.Delta;
            if (NextAttack <= 0)
            {
                double distsq;
                PlayerEntity player = NearestPlayer(out distsq);
                if (distsq < 10 * 10)
                {
                    Location target = player.GetCenter();
                    Location pos = GetEyePosition();
                    Location rel = (target - pos).Normalize();
                    Direction = Utilities.VectorToAngles(rel);
                    Items.Items[0].Info.Click(this, Items.Items[0]);
                    Items.Items[0].Info.ReleaseClick(this, Items.Items[0]);
                }
                NextAttack = Utilities.UtilRandom.NextDouble();
            }
        }

        public PlayerEntity NearestPlayer(out double distSquared)
        {
            PlayerEntity player = null;
            double distsq = double.MaxValue;
            Location p = GetCenter();
            foreach (PlayerEntity tester in TheRegion.Players)
            {
                double td = (tester.GetCenter() - p).LengthSquared();
                if (td < distsq)
                {
                    player = tester;
                    distsq = td;
                }
            }
            distSquared = distsq;
            return player;
        }
        
        public override void Die()
        {
            if (Removed)
            {
                return;
            }
            TheRegion.Explode(GetPosition(), 5);
            RemoveMe();
        }
    }
}
