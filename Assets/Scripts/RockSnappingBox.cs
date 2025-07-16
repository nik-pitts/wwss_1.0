using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RockSnappingBox : MonoBehaviour
{
    private bool wasSet = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rock"))
        {
            Debug.Log("Rock detected");
            other.transform.SetParent(transform.parent);
            if (!wasSet)
            {
                wasSet = true;
                other.GetComponent<ObjectGrabbable>().isInteractable = false;
                other.transform.localPosition = transform.localPosition;
            }
        }
    }
}
