using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct UnitMoveAlongHeading : IJobParallelFor
{
    public float DeltaTime;
    public NativeArray<UnitBoidData> Boids;
    public bool ClumpNearTarget;

    [BurstCompile]
    public void Execute(int i)
    {
        UnitBoidData movementData = Boids[i];
        if (movementData.hasTarget)
        {
            // Handle Steering Behaviour Generation
            float3 offset = movementData.target - movementData.position;
            offset.y = 0; // Assuming movement on the XZ plane
            float distanceToTarget = math.length(offset);

            //float predSpeedFactor = 1f;
            
            float predSpeedFactor = 1f;

            if (ClumpNearTarget)
            {
                if (distanceToTarget < 2f)
                {
                    predSpeedFactor = distanceToTarget / 2f;
                    if (distanceToTarget < 0.1f) predSpeedFactor = 0f;
                }
            }

            movementData.predictedSpeedFactor = math.lerp(movementData.predictedSpeedFactor,
                predSpeedFactor, DeltaTime * 10f);

            Debug.DrawRay(movementData.position, Vector3.up * 2f,
                Color.Lerp(Color.green, Color.red, movementData.speedFactor));

            float speedFactor = movementData.speedFactor;

            float3 composedHeading = Unity.Mathematics.math.normalizesafe(offset + movementData.avoidanceHeading);

            quaternion targetRotation = quaternion.LookRotationSafe(composedHeading, math.up());
            movementData.rotation = math.slerp(movementData.rotation, targetRotation,
                DeltaTime * speedFactor * movementData.rotationalSpeed);

            float3 acceleration = math.forward(movementData.rotation);

            if (distanceToTarget < movementData.boidSize)
            {
                acceleration = math.clamp(offset, -1f, 1f);
            }


            float3 dir = acceleration * movementData.speedFactor *
                         DeltaTime * movementData.movementSpeed +
                         movementData.avoidanceHeading * DeltaTime;

            // clamp dir to max speed
            dir = math.clamp(dir, -movementData.movementSpeed * DeltaTime, movementData.movementSpeed * DeltaTime);

            movementData.nextPosition =
                movementData.position + dir;
        }
        else
        {
            float3 dir = movementData.avoidanceHeading *
                         DeltaTime;
            dir = math.clamp(dir, -movementData.movementSpeed * DeltaTime, movementData.movementSpeed * DeltaTime);
            movementData.nextPosition = movementData.position + dir;
        }

        Boids[i] = movementData;
    }
}