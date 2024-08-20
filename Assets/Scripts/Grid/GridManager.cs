using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct GridData
{
    public Vector2Int gridSize;
    public float cellSize;
    public float3 min;
    public float3 max;
    public float3 origin;
}


[System.Serializable]
public struct GridCellData
{
    public float3 worldPosition;
    public int2 gridPosition;
}

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize;
    public float cellSize;
    public Vector3 origin;
    public GridData gridData;

    public static GridManager Instance;

    private UnsafeParallelHashMap<int, GridCellData> _gridCells;

    private void Awake()
    {
        Instance = this;
        gridData = new GridData
        {
            gridSize = gridSize,
            cellSize = cellSize,
            origin = origin
        };

        _gridCells = new UnsafeParallelHashMap<int, GridCellData>(gridData.gridSize.x * gridData.gridSize.y,
            Allocator.Persistent);

        SetupGridCells();
    }

    private void Start()
    {
        SetupGridItems();
    }

    public static GridData GridData()
    {
        return Instance.gridData;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Gizmos.color = Color.green;
                Vector3 cellCenter =
                    origin + new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2);
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }

    public void SetupGridCells()
    {
        for (int x = 0; x < gridData.gridSize.x; x++)
        {
            for (int y = 0; y < gridData.gridSize.y; y++)
            {
                GridCellData data = new GridCellData();

                data.worldPosition = gridData.origin + new float3(x * gridData.cellSize + gridData.cellSize / 2, 0,
                    y * gridData.cellSize + gridData.cellSize / 2);

                data.gridPosition = new int2(x, y);

                _gridCells.TryAdd(x * gridData.gridSize.y + y, data);
            }
        }

        gridData.min = gridData.origin;
        gridData.max = gridData.origin + new float3(gridData.gridSize.x * gridData.cellSize, 0,
            gridData.gridSize.y * gridData.cellSize) + new float3(0.0f, 100f, 0.0f);
    }

    public static int2 GetGridPos(float3 position, GridData gridData)
    {
        var relativePosition = position - gridData.origin;

        var x = (int)math.clamp((int)(relativePosition.x / gridData.cellSize), 0, gridData.gridSize.x - 1);
        var y = (int)math.clamp((int)(relativePosition.z / gridData.cellSize), 0, gridData.gridSize.y - 1);

        // check if in bounds
        if (x < 0 || x >= gridData.gridSize.x || y < 0 || y >= gridData.gridSize.y)
        {
            return -1;
        }

        return new int2(x, y);
    }

    public static int GetGridIndex(float3 position, GridData gridData)
    {
        var relativePosition = position - gridData.origin;

        var x = (int)math.clamp((int)(relativePosition.x / gridData.cellSize), 0, gridData.gridSize.x - 1);
        var y = (int)math.clamp((int)(relativePosition.z / gridData.cellSize), 0, gridData.gridSize.y - 1);

        // check if in bounds
        if (x < 0 || x >= gridData.gridSize.x || y < 0 || y >= gridData.gridSize.y)
        {
            return -1;
        }

        return x * gridData.gridSize.y + y;
    }

    public static float3 GetWorldPos(int x, int y, GridData gridData)
    {
        return gridData.origin + new float3(x * gridData.cellSize + gridData.cellSize / 2, 0,
            y * gridData.cellSize + gridData.cellSize / 2);
    }

    public static int FlattenGridIndex(int x, int y, GridData gridData)
    {
        return x * gridData.gridSize.y + y;
    }

    public void SetupGridItems()
    {
        GridItem[] gridItems = FindObjectsOfType<GridItem>();

        foreach (var item in gridItems)
        {
            Transform transform = item.transform;
            GridItemData data = item.Data;

            var originGridPos = GetGridPos(transform.position, gridData);
            var originGridPosition = GetWorldPos(originGridPos.x, originGridPos.y, gridData);

            if (item.snapToGrid)
                transform.position = new float3(originGridPosition.x, transform.position.y, originGridPosition.z);


            float3 worldPos = transform.TransformPoint(data.Offset);

            float3 modifiedBounds = data.Bounds;

            modifiedBounds.x = Mathf.FloorToInt(data.Bounds.x / cellSize) * cellSize;
            modifiedBounds.y = Mathf.FloorToInt(data.Bounds.y / cellSize) * cellSize;


            var t_min = worldPos - modifiedBounds / 2;
            var t_max = worldPos + modifiedBounds / 2;

            var min = math.min(t_min, t_max);
            var max = math.max(t_min, t_max);

            int maxOccupancyX = Mathf.Max(Mathf.RoundToInt(data.Bounds.x / cellSize), 0);
            int maxOccupancyY = Mathf.Max(Mathf.RoundToInt(data.Bounds.y / cellSize), 0);


            int2 minGrid = GetGridPos(min, gridData);
            int2 maxGrid = GetGridPos(max, gridData);

            // int2 centralGrid = ECSGridManager.GetGridPos(worldPos, GridData);
            // int2 minGrid = new int2(centralGrid.x - Mathf.FloorToInt(maxOccupancyX / 2),
            //     centralGrid.y - Mathf.FloorToInt(maxOccupancyY / 2));
            // int2 maxGrid = new int2(centralGrid.x + Mathf.FloorToInt(maxOccupancyX / 2),
            //     centralGrid.y + Mathf.FloorToInt(maxOccupancyY / 2));

            item.containingCells.Clear();
            for (int x = minGrid.x; x <= maxGrid.x; x++)
            {
                for (int y = minGrid.y; y <= maxGrid.y; y++)
                {
                    int gridPosition = FlattenGridIndex(x, y, gridData);
                    item.containingCells.Add(gridPosition);
                }
            }
        }
    }

    private void OnDestroy()
    {
        _gridCells.Dispose();
    }
}