using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using BoxCollider = UnityEngine.BoxCollider;
using Collider = UnityEngine.Collider;
using SphereCollider = UnityEngine.SphereCollider;

public class UnitManager : MonoBehaviour
{
    [Header("Movement Options")]
    public bool moveToClick = true;
    public bool clumpAroundTarget = false;
    public float avoidanceForce = 20f;
    
    private NativeList<RigidBody> _rigidBodies;
    private NativeParallelMultiHashMap<int, int> _hashedRigidBodies;
    public LayerMask collisionMask;

    public List<Unit> units = new List<Unit>();
    
    
    private UnitBoidSystem _unitBoidSystem = new UnitBoidSystem();
    private bool startSimulation = false;

    public static UnitManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        BuildUnitPhysicsWorld();
        units = new List<Unit>(GameObject.FindObjectsOfType<Unit>());
        
        yield return new WaitForEndOfFrame();
        startSimulation = true;
    }
    
    private void BuildUnitPhysicsWorld()
    {
        int colliderCount = GameObject.FindObjectsOfType<SphereCollider>().Length;
        int boxColliderCount = GameObject.FindObjectsOfType<BoxCollider>().Length;
        
        _rigidBodies = new NativeList<RigidBody>(colliderCount+boxColliderCount, Allocator.Persistent);
        _hashedRigidBodies = new NativeParallelMultiHashMap<int, int>(GridManager.GridData().gridSize.x * GridManager.GridData().gridSize.y, Allocator.Persistent);
        var list = GameObject.FindObjectsOfType<SphereCollider>();

        for (int i = 0; i < list.Length; i++)
        {
            var sphereCollider = list[i];
            if ((collisionMask.value & (1 << sphereCollider.gameObject.layer)) == 0)
                continue;

            GridItem item = sphereCollider.transform.GetComponentInParent<GridItem>();
            if (item == null) item = sphereCollider.transform.GetComponent<GridItem>();

            if (item == null) continue;
            
            var sphereGeometry = new Unity.Physics.SphereGeometry()
            {
                Radius = sphereCollider.radius,
                Center = sphereCollider.center 
            };

            var colliderBlob = Unity.Physics.SphereCollider.Create(sphereGeometry,
                Unity.Physics.CollisionFilter.Default, Unity.Physics.Material.Default);

            var sphereColliderTransform = sphereCollider.transform;
            var rBody = new RigidBody
            {
                Collider = colliderBlob,
                WorldFromBody = new RigidTransform
                {
                    pos = sphereColliderTransform.position,
                    rot = sphereColliderTransform.rotation
                },
                Entity = Entity.Null,
                Scale = sphereColliderTransform.localScale.x,
                CustomTags = 1
            };

            _rigidBodies.Add(rBody);

            foreach (var gridCell in item.containingCells)
            {
                _hashedRigidBodies.Add(gridCell, _rigidBodies.Length - 1);
            }

        }
    }

    public Vector3 target;
    
    private void Update()
    {
        if (startSimulation)
        {
            // follow the target
            if (Input.GetMouseButtonDown(0) && moveToClick)
            {
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    target = hit.point;
                }
                
                for (int i = 0; i < units.Count; i++)
                {
                    units[i].MoveTo(target.x, target.z);
                }
            }

         

            NativeArray<UnitBoidData> boids =
                new NativeArray<UnitBoidData>(units.Count, Allocator.TempJob);
            var hashMap =
                new NativeParallelMultiHashMap<int, UnitBoidData>(
                    GridManager.GridData().gridSize.x * GridManager.GridData().gridSize.y,
                    Allocator.TempJob);

            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                {
                    units[i].ApplyDataToBoid();
                    boids[i] = units[i].boidData;
                    var bd = units[i].boidData;

                    int gridIndex = GridManager.GetGridIndex(bd.position, GridManager.GridData());

                    hashMap.Add(gridIndex, boids[i]);
                }
                else
                {
                    boids[i] = new UnitBoidData();
                }
            }

            _unitBoidSystem.Execute(ref boids, _rigidBodies, _hashedRigidBodies, ref hashMap, clumpAroundTarget, avoidanceForce);

            for (int i = 0; i < units.Count; i++)
            {
                units[i].ApplyBoidData(boids[i]);
            }

            boids.Dispose();
            hashMap.Dispose();
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _rigidBodies.Length; i++)
        {
            _rigidBodies[i].Collider.Dispose();
        }
        
        _rigidBodies.Dispose();
        _hashedRigidBodies.Dispose();
    }

}
