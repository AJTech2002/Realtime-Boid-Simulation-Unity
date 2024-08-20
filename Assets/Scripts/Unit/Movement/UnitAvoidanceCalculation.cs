using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
    public struct UnitBoidAvoidanceCalculationJob : IJobParallelFor
    {
        public float DeltaTime;
        [ReadOnly] public NativeParallelMultiHashMap<int, UnitBoidData> HashedEntities;
        public NativeArray<UnitBoidData> boids;
        [ReadOnly] public GridData data;

        public float AvoidanceForce;

        [ReadOnly] public Unity.Mathematics.Random random;
        [BurstCompile]
        public void Execute(int index)
        {
            UnitBoidData currentBoid = boids[index];

            float3 boidAvoidanceDirection = float3.zero;
            float averageNeighbourGoingToSamePlaceSpeedFactor = currentBoid.predictedSpeedFactor;
            float myDistanceToTarget = math.distance(currentBoid.target, currentBoid.position);

            float3 zeroObst = random.NextFloat3Direction();
            zeroObst.y = 0;
            var center = GridManager.GetGridPos(currentBoid.position, data);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var neighbour = center + new int2(x, y);
                    int neighboutIndex = GridManager.FlattenGridIndex(neighbour.x, neighbour.y, data);

                    if (HashedEntities.TryGetFirstValue(neighboutIndex, out UnitBoidData neighbourBoid, out var iterator))
                    {
                        do
                        {
                            int entity = neighbourBoid.id;
                            if (entity != currentBoid.id)
                            {
                                // Get the translation component
                                float3 translation = neighbourBoid.position;
                                float3 obstacleOffset = translation - currentBoid.position;
                                

                                float distanceToObstacle = math.length(obstacleOffset);
                                float otherBoidRadius = neighbourBoid.boidSize;

                                // check if my radius is within the other boid's radius
                                //&& otherBoidRadius >= boidData.BoidSize
                                if (distanceToObstacle < otherBoidRadius + currentBoid.boidSize )
                                {
                                    obstacleOffset.y = 0f;
                                    float avoidancePerc = Unity.Mathematics.math.clamp(
                                        otherBoidRadius - distanceToObstacle,
                                        0f, 1f);
                                    float3 avoidance = math.normalizesafe(obstacleOffset);

                                    if (math.length(obstacleOffset) == 0)
                                    {
                                        boidAvoidanceDirection -= zeroObst * avoidancePerc;
                                 
                                    }
                                    else
                                    {
                                        boidAvoidanceDirection -= avoidance * avoidancePerc * math.clamp(currentBoid.boidSize/otherBoidRadius, 0.04f, 10f);
                                    }
                                    
                                    if (currentBoid.hasTarget && avoidancePerc < 0.7f)
                                    {
                                        float boidDistanceToTarget =
                                            math.distance(neighbourBoid.target, neighbourBoid.position);
                                    
                                        if (math.distance(currentBoid.target, neighbourBoid.target) < 0.1f &&
                                            boidDistanceToTarget < myDistanceToTarget)
                                        {
                                            // Always have them slowly clump in together if they are going to the same place
                                            averageNeighbourGoingToSamePlaceSpeedFactor =
                                                math.max(neighbourBoid.speedFactor, 0.2f);
                                            // WAS 0.2
                                        }
                                    }
                                }
                                // otherwise prioritise avoidance if < 0.7 (very close!)
                               
                            
                            }
                        } while (HashedEntities.TryGetNextValue(out neighbourBoid, ref iterator));
                    }
                }
            }


            currentBoid.avoidanceHeading = boidAvoidanceDirection * AvoidanceForce;

            if (currentBoid.hasTarget)
            {
                float speedFactor = math.lerp(currentBoid.speedFactor, averageNeighbourGoingToSamePlaceSpeedFactor,
                    DeltaTime * 15f);

                currentBoid.speedFactor = speedFactor;
            }

            boids[index] = currentBoid;
        }
    }