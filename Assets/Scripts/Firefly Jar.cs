using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyJar : MonoBehaviour
{
    [SerializeField] float lightUpTime = 3.0f; // Time in seconds to light up the jar
    float onStayTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other != null && other.CompareTag("Firefly"))
        {
            onStayTime += Time.deltaTime;
            // light up the jar when a firefly is inside for more than the specified time
            if (onStayTime > lightUpTime)
            {
                Debug.Log("Firefly detected, lighting up the jar");
                Renderer jarRenderer = GetComponent<Renderer>();
                Material jarMaterial = jarRenderer.material;
                jarMaterial.EnableKeyword("_EMISSION");
                StartCoroutine(ScaleDown(other.transform, 2f));
            }
        }
    }
    private IEnumerator ScaleDown(Transform target, float scaleTime)
    {
        if (target == null) yield break; // Exit if already destroyed
        
        Vector3 originalScale = target.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < scaleTime)
        {
            if (target == null) yield break; // Check if destroyed during coroutine
            
            elapsedTime += Time.deltaTime;
            float scaleMultiplier = Mathf.Lerp(1f, 0f, elapsedTime / scaleTime);
            
            target.localScale = originalScale * scaleMultiplier;
            
            yield return null;
        }
        
        // Final null check before destroying
        if (target != null)
        {
            Destroy(target.gameObject);
        }
    }
}

