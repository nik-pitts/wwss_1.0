using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ThrowableObject : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasLanded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasLanded && other.CompareTag("Terrain"))
        {
            Land();
        }
    }

    private void Land()
    {
        hasLanded = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position += Vector3.up * 0.3f;
    }
}
