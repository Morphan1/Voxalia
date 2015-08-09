using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries.Events;
using BEPUphysics.OtherSpaceStages;

namespace Voxalia.Shared.Collision
{
    public class FullChunkObject : StaticCollidable
    {
        public FullChunkObject(Vector3 pos, BlockInternal[] blocks)
        {
            ChunkShape = new FullChunkShape(blocks);
            Position = pos;
        }

        public FullChunkShape ChunkShape;

        public Vector3 Position;

        public ContactEventManager<FullChunkObject> Events = new ContactEventManager<FullChunkObject>();

        protected override IContactEventTriggerer EventTriggerer
        {
            get { return Events; }
        }

        protected override IDeferredEventCreator EventCreator
        {
            get { return Events; }
        }

        public override void UpdateBoundingBox()
        {
            boundingBox = new BoundingBox(Position, Position + new Vector3(30, 30, 30));
        }

        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, out RayHit hit)
        {
            // TODO: Actual code, less cheating!
            Vector3 move = sweep - startingTransform.Position;
            float movelen = move.Length();
            Ray testray = new Ray(startingTransform.Position, move / movelen);
            return RayCast(testray, movelen, out hit);
        }

        public override bool RayCast(Ray ray, float maximumLength, out RayHit rayHit)
        {
            Ray r2 = new Ray(ray.Position - Position, ray.Direction);
            if (ChunkShape.RayCast(ref r2, maximumLength, out rayHit))
            {
                rayHit.Location = rayHit.Location + ray.Position;
                rayHit.T = (rayHit.Location - r2.Position).Length() / maximumLength; // TODO: Determine if needed at all. Length()'s are bad!
                // TODO: ALTERNATELY, find a quicker calculation for T.
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
