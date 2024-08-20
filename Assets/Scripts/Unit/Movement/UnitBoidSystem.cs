using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct UnitBoidData
{
    public int id;
    public float3 position;
    public float speedFactor;
    public float3 avoidanceHeading;
    public bool hasTarget;
    public float3 target;
    public float predictedSpeedFactor;
    public float3 nextPosition;
    public float movementSpeed;
    public float rotationalSpeed;
    public float boidSize;
    public quaternion rotation;
}


[BurstCompile]
public class UnitBoidSystem
{
    [BurstCompile]
    public void Execute(ref NativeArray<UnitBoidData> inputBoids, NativeArray<RigidBody> rigidBodies,
        NativeParallelMultiHashMap<int, int> hashedRigidBodies,
        ref NativeParallelMultiHashMap<int, UnitBoidData> hashMap, bool clumpNearTarget, float avoidanceForce)
    {
        float deltaTime = math.min(0.05f, Time.deltaTime);

        var r = new Unity.Mathematics.Random((uint)Time.frameCount);

        var avoidanceCalculationJob = new UnitBoidAvoidanceCalculationJob
        {
            random = r,
            DeltaTime = deltaTime,
            HashedEntities = hashMap,
            boids = inputBoids,
            data = GridManager.GridData(),
            AvoidanceForce = avoidanceForce,
        };

        var moveAlongHeadingJob = new UnitMoveAlongHeading
        {
            DeltaTime = deltaTime,
            Boids = inputBoids,
            ClumpNearTarget = clumpNearTarget
        };

        var collisionAvoidanceJob = new UnitCollisionJob
        {
            GridData = GridManager.GridData(),
            TimeStep = deltaTime,
            boidData = inputBoids,
            rigidBodies = rigidBodies,
            hashedRigidBodies = hashedRigidBodies,
        };

        if (Time.frameCount % 2 == 0)
        {
            var a = avoidanceCalculationJob.Schedule(inputBoids.Length, 32);
            var b = moveAlongHeadingJob.Schedule(inputBoids.Length, 32, a);
            collisionAvoidanceJob.Schedule(inputBoids.Length, 32, b).Complete();
        }
        else
        {
            var b = moveAlongHeadingJob.Schedule(inputBoids.Length, 32);
            collisionAvoidanceJob.Schedule(inputBoids.Length, 32, b).Complete();
        }
    }
}