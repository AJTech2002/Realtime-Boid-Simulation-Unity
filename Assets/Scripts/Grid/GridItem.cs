using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct GridItemData
{
    public float3 Offset;
    public float3 Bounds;
}
    
public class GridItem : MonoBehaviour
{
    public bool snapToGrid = true;
    public GridItemData Data;
        
    public List<int> containingCells = new List<int>(); 

    private void Awake()
    {
        Collider collider = GetComponentInChildren<Collider>();

        Data = new GridItemData
        {
            Offset = Vector3.zero,
            Bounds = collider.bounds.size,
        };
    }
        
}