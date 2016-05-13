using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;
using BEPUutilities;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SpawnCharacterPacketOut: AbstractPacketOut
    {
        public SpawnCharacterPacketOut(CharacterEntity ent)
        {
            ID = 38;
            DataStream ds = new DataStream();
            DataWriter dr = new DataWriter(ds);
            dr.WriteBytes(ent.GetPosition().ToBytes());
            Quaternion quat = ent.GetOrientation();
            dr.WriteFloat(quat.X);
            dr.WriteFloat(quat.Y);
            dr.WriteFloat(quat.Z);
            dr.WriteFloat(quat.W);
            dr.WriteLong(ent.EID);
            dr.WriteFloat(ent.GetMass());
            dr.WriteFloat(ent.CBAirForce);
            dr.WriteFloat(ent.CBAirSpeed);
            dr.WriteFloat(ent.CBCrouchSpeed);
            dr.WriteFloat(ent.CBDownStepHeight);
            dr.WriteFloat(ent.CBGlueForce);
            dr.WriteFloat(ent.CBHHeight);
            dr.WriteFloat(ent.CBJumpSpeed);
            dr.WriteFloat(ent.CBMargin);
            dr.WriteFloat(ent.CBMaxSupportSlope);
            dr.WriteFloat(ent.CBMaxTractionSlope);
            dr.WriteFloat(ent.CBProneSpeed);
            dr.WriteFloat(ent.CBRadius);
            dr.WriteFloat(ent.CBSlideForce);
            dr.WriteFloat(ent.CBSlideJumpSpeed);
            dr.WriteFloat(ent.CBSlideSpeed);
            dr.WriteFloat(ent.CBStandSpeed);
            dr.WriteFloat(ent.CBStepHeight);
            dr.WriteFloat(ent.CBTractionForce);
            dr.WriteFloat(ent.mod_xrot);
            dr.WriteFloat(ent.mod_yrot);
            dr.WriteFloat(ent.mod_zrot);
            dr.WriteFloat(ent.mod_scale);
            dr.WriteInt(ent.mod_color.ToArgb());
            byte dtx = 0;
            if (ent.Visible)
            {
                dtx |= 1;
            }
            if (ent.CGroup == CollisionUtil.Solid)
            {
                dtx |= 2;
            }
            else if (ent.CGroup == CollisionUtil.NonSolid)
            {
                dtx |= 4;
            }
            else if (ent.CGroup == CollisionUtil.Item)
            {
                dtx |= 2 | 4;
            }
            else if (ent.CGroup == CollisionUtil.Player)
            {
                dtx |= 8;
            }
            else if (ent.CGroup == CollisionUtil.Water)
            {
                dtx |= 2 | 8;
            }
            else if (ent.CGroup == CollisionUtil.WorldSolid)
            {
                dtx |= 2 | 4 | 8;
            }
            else if (ent.CGroup == CollisionUtil.Character)
            {
                dtx |= 16;
            }
            dr.Write(dtx);
            dr.WriteInt(ent.TheServer.Networking.Strings.IndexForString(ent.model));
            dr.Flush();
            Data = ds.ToArray();
            dr.Close();
        }
    }
}
