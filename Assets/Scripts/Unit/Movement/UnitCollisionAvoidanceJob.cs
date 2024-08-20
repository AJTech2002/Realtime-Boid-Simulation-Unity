using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
public partial struct UnitCollisionJob : IJobParallelFor
{
    [Unity.Collections.ReadOnly] public GridData GridData;
    [Unity.Collections.ReadOnly] public float TimeStep;
    public NativeArray<UnitBoidData> boidData;
    [ReadOnly] public NativeArray<RigidBody> rigidBodies;
    [ReadOnly] public NativeParallelMultiHashMap<int, int> hashedRigidBodies;

    [BurstCompile]
    public void Execute(int index)
    {
        UnitBoidData unitData = boidData[index];

        if (math.lengthsq(unitData.nextPosition) < 0.01f)
        {
            return;
        }

        float3 projectedPos = unitData.nextPosition;

        float3 min = GridData.min;
        float3 max = GridData.max;
        

        projectedPos = math.clamp(projectedPos, min, max);

        PointDistanceInput circle = new PointDistanceInput
        {
            Position = projectedPos,
            MaxDistance = unitData.boidSize / 2f,
            Filter = CollisionFilter.Default
        };


        float3 collisionAvoidance = float3.zero;

        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(20, Allocator.TempJob);
        bool hasHit = false;

        var center = GridManager.GetGridPos(projectedPos, GridData);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var neighbour = center + new int2(x, y);
                int neighboutIndex = GridManager.FlattenGridIndex(neighbour.x, neighbour.y, GridData);

                if (hashedRigidBodies.TryGetFirstValue(neighboutIndex, out int rBodyIndex, out var iterator))
                {
                    do
                    {
                        RigidBody rBody = rigidBodies[rBodyIndex];


                        if (rBody.CalculateDistance(circle, ref hits))
                        {
                            hasHit = true;
                        }
                    } while (hashedRigidBodies.TryGetNextValue(out rBodyIndex, ref iterator));
                }
            }
        }

        if (hasHit)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                DistanceHit hit = hits[i];
                float3 avoidance = hit.Position - circle.Position;
                collisionAvoidance -=
                    math.normalizesafe(avoidance) * (circle.MaxDistance - math.length(avoidance));

                if (hit.Distance < 0f)
                {
                    collisionAvoidance +=
                        math.normalizesafe(avoidance) * (circle.MaxDistance - math.length(avoidance));
                }
            }
        }

        hits.Dispose();

        if (math.length(collisionAvoidance) > 0)
        {
            collisionAvoidance.y = 0f;


            float3 newPos = collisionAvoidance * TimeStep * 15f + projectedPos;
            newPos = math.clamp(newPos, GridData.min, GridData.max);
            unitData.nextPosition = newPos;

            unitData.predictedSpeedFactor =
                math.lerp(unitData.predictedSpeedFactor, 0.2f, TimeStep * 20f);

            quaternion targetRotation = quaternion.LookRotationSafe(collisionAvoidance, math.up());
            unitData.rotation = math.slerp(unitData.rotation, targetRotation,
                TimeStep * unitData.rotationalSpeed);
        }
        else
        {
            unitData.nextPosition = projectedPos;
        }

        boidData[index] = unitData;
    }
}