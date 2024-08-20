using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitBoidData boidData = new UnitBoidData();
    public static int IDCounter = 0;
    private void Awake()
    {
        boidData.id = IDCounter++;
    }

    public void ApplyDataToBoid()
    {
        boidData.position = transform.position;
        boidData.rotation = transform.rotation;
    }
    
    public void ApplyBoidData (UnitBoidData data)
    {
        transform.position = data.nextPosition;
        transform.rotation = data.rotation;
        boidData.speedFactor = data.speedFactor;
        boidData.avoidanceHeading = data.avoidanceHeading;
        boidData.predictedSpeedFactor = data.predictedSpeedFactor;
        boidData.position = transform.position;
        boidData.rotation = transform.rotation;
        
    }
    
    public void MoveTo(float x, float y)
    {
        boidData.target = new float3(x, 0f, y);
        boidData.hasTarget = true;
    }
    
    public void Stop()
    {
        boidData.hasTarget = false;
    }
    
}
