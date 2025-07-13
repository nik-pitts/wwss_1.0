using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class RockContainer : MonoBehaviour
{
    // Start is called before the first frame update
    public int collectedNumberofRocks = 0;
    private List<GameObject> collectedRocks = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rock"))
        {
            Debug.Log("Rock detected");
            if (!collectedRocks.Contains(other.gameObject))
            {
                collectedRocks.Add(other.gameObject);
                collectedNumberofRocks++;
                Debug.Log("Rock collected! Total rocks: " + collectedNumberofRocks);
            }
        }
    }
}
