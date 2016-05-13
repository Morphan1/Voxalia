using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;
using BEPUutilities;
using OpenTK;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnCharacterPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            GenericCharacterEntity ent = new GenericCharacterEntity(TheClient.TheRegion);
            ent.SetPosition(Location.FromBytes(dr.ReadBytes(12), 0));
            ent.SetOrientation(new BEPUutilities.Quaternion(dr.ReadFloat(), dr.ReadFloat(), dr.ReadFloat(), dr.ReadFloat()));
            ent.EID = dr.ReadLong();
            ent.SetMass(dr.ReadFloat());
            ent.CBAirForce = dr.ReadFloat();
            ent.CBAirSpeed = dr.ReadFloat();
            ent.CBCrouchSpeed = dr.ReadFloat();
            ent.CBDownStepHeight = dr.ReadFloat();
            ent.CBGlueForce = dr.ReadFloat();
            ent.CBHHeight = dr.ReadFloat();
            ent.CBJumpSpeed = dr.ReadFloat();
            ent.CBMargin = dr.ReadFloat();
            ent.CBMaxSupportSlope = dr.ReadFloat();
            ent.CBMaxTractionSlope = dr.ReadFloat();
            ent.CBProneSpeed = dr.ReadFloat();
            ent.CBRadius = dr.ReadFloat();
            ent.CBSlideForce = dr.ReadFloat();
            ent.CBSlideJumpSpeed = dr.ReadFloat();
            ent.CBSlideSpeed = dr.ReadFloat();
            ent.CBStandSpeed = dr.ReadFloat();
            ent.CBStepHeight = dr.ReadFloat();
            ent.CBTractionForce = dr.ReadFloat();
            ent.PreRot *= Matrix4.CreateRotationX(dr.ReadFloat() * (float)Utilities.PI180);
            ent.PreRot *= Matrix4.CreateRotationY(dr.ReadFloat() * (float)Utilities.PI180);
            ent.PreRot *= Matrix4.CreateRotationZ(dr.ReadFloat() * (float)Utilities.PI180);
            ent.mod_scale = dr.ReadFloat();
            ent.PreRot = Matrix4.CreateScale(ent.mod_scale) * ent.PreRot;
            ent.color = System.Drawing.Color.FromArgb(dr.ReadInt());
            byte dtx = dr.ReadByte();
            ent.Visible = (dtx & 1) == 1;
            int solidity = (dtx & (2 | 4 | 8));
            if (solidity == 2)
            {
                ent.CGroup = CollisionUtil.Solid;
            }
            else if (solidity == 4)
            {
                ent.CGroup = CollisionUtil.NonSolid;
            }
            else if (solidity == (2 | 4))
            {
                ent.CGroup = CollisionUtil.Item;
            }
            else if (solidity == 8)
            {
                ent.CGroup = CollisionUtil.Player;
            }
            else if (solidity == (2 | 8))
            {
                ent.CGroup = CollisionUtil.Water;
            }
            else if (solidity == (2 | 4 | 8))
            {
                ent.CGroup = CollisionUtil.WorldSolid;
            }
            else if (solidity == 16)
            {
                ent.CGroup = CollisionUtil.Character;
            }
            ent.model = TheClient.Models.GetModel(TheClient.Network.Strings.StringForIndex(dr.ReadInt()));
            ent.TheRegion.SpawnEntity(ent);
            dr.Close();
            return true;
        }
    }
}
