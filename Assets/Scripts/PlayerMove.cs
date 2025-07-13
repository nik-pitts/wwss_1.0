using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float eyeHeight = 1.6f; // Height of the player's eyes above the ground
    private Vector3 currentPos;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currentPos = transform.position;
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
