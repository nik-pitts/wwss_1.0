using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PlayerPatroll : MonoBehaviour
{
    private NavMeshAgent navMeshPlayer;
    [SerializeField] private LayerMask terrainLayer;
    
    // patrol
    private Vector3 destinationPoint;
    private bool walkPointSet;
    [SerializeField] private float walkRange;

    private void Start()
    {
        navMeshPlayer = GetComponent<NavMeshAgent>();
        Debug.Log(terrainLayer.value);
    }

    private void Update()
    {
        patrol();
    }

    void patrol()
    {
        if (!walkPointSet)
        {
            searchDestination();
        }
        if (walkPointSet)
        {
            Debug.Log(navMeshPlayer.SetDestination(destinationPoint));
        }

        if (Vector3.Distance(transform.position, destinationPoint) < 10)
        {
            walkPointSet = false;
        }
    }

    void searchDestination()
    {
        float z = Random.Range(-walkRange, walkRange);
        float x = Random.Range(-walkRange, walkRange);

        destinationPoint = new Vector3(transform.position.x + x, 
                                         transform.position.y, 
                                        transform.position.z + z);
        
        if (Physics.Raycast(destinationPoint, Vector3.down, terrainLayer))
        {
            walkPointSet = true;
        }
    }
    
}
