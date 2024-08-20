using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Circle,
    Square
}


public class Formation : MonoBehaviour
{
    public Transform center;
    private List<Unit> _units = new List<Unit>();

    public FormationType formationType;
    
    [Header("Circle Formation")] 
    public float radius;
    
    [Header("Square Formation")] 
    public float width;
    public float height;
    
    private void Start()
    {
        _units = new List<Unit>(transform.GetComponentsInChildren<Unit>());
    }

    private void Update()
    {
        if (center == null)
            return;
        
        switch (formationType)
        {
            case FormationType.Circle:
                CircleFormation();
                break;
            case FormationType.Square:
                SquareFormation();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void CircleFormation()
    {
        float angle = 0;
        float angleIncrease = 360 / _units.Count;
        
        foreach (var unit in _units)
        {
            float x = center.position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = center.position.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            unit.MoveTo(x, z);
            Debug.DrawLine(center.position, new Vector3(x,center.position.y,z), Color.black);

            angle += angleIncrease;
        }
        
        // Vector3 forward = center.forward;
        // for (int i = 0; i < _units.Count; i++)
        // {
        //     Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
        //     _units[i].MoveTo(offset.x, offset.z);
        //     angle += angleIncrease;
        // }
    }
    
    private void SquareFormation()
    {
        int row = 0;
        int column = 0;
        int unitCount = 0;
        
        foreach (var unit in _units)
        {
            float x = center.position.x + column * width;
            float z = center.position.z + row * height;

            unit.MoveTo(x, z);
            Debug.DrawRay(new Vector3(x,center.position.y,z), Vector3.up*5f, Color.black);

            column++;
            unitCount++;
            if (unitCount % 5 == 0)
            {
                row++;
                column = 0;
            }
        }
    }
}
