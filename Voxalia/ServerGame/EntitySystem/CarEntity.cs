using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.OtherSystems;
using BEPUutilities;
using BEPUphysics.Constraints.TwoEntity.JointLimits;
using BEPUphysics.Constraints.TwoEntity.Motors;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using FreneticScript;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class CarEntity: VehicleEntity
    {
        public CarEntity(string vehicle, Region tregion)
            : base(vehicle, tregion)
        {
        }

        public override EntityType GetEntityType()
        {
            return EntityType.CAR;
        }

        public override BsonDocument GetSaveData()
        {
            // TODO: Save properly!
            return null;
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            HandleWheels();
        }

        public override void HandleInput(CharacterEntity character)
        {
            // TODO: Dynamic multiplier values.
            foreach (JointVehicleMotor motor in DrivingMotors)
            {
                motor.Motor.Settings.VelocityMotor.GoalVelocity = character.YMove * 100;
            }
            foreach (JointVehicleMotor motor in SteeringMotors)
            {
                motor.Motor.Settings.Servo.Goal = MathHelper.Pi * -0.2f * character.XMove;
            }
        }
    }
}
