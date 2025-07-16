using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float eyeHeight = 1.6f;
    private Vector3 lastPosition;
    
    void Start()
    {
        lastPosition = transform.position;
        AdjustToTerrain();
    }

    void Update()
    {
        // Only adjust if player has moved in X or Z
        if (HasMovedHorizontally())
        {
            AdjustToTerrain();
            lastPosition = transform.position;
        }
    }
    
    private bool HasMovedHorizontally()
    {
        Vector3 currentPos = transform.position;
        return Mathf.Abs(currentPos.x - lastPosition.x) > 0.01f || 
               Mathf.Abs(currentPos.z - lastPosition.z) > 0.01f;
    }
    
    private void AdjustToTerrain()
    {
        Vector3 currentPos = transform.position;
        Vector3 terrainPos = GetTerrainPos(currentPos.x, currentPos.z);
        transform.position = new Vector3(currentPos.x, terrainPos.y + eyeHeight, currentPos.z);
    }
    
    private Vector3 GetTerrainPos(float x, float z)
    {
        RaycastHit hit;
        Vector3 origin = new Vector3(x, 100f, z);
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("NavMesh")))
        {
            return hit.point; 
        }
        return new Vector3(x, 0, z);
    }
}