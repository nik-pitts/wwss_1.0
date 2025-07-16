using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyJar : MonoBehaviour
{
    [SerializeField] float lightUpTime = 3.0f; // Time in seconds to light up the jar
    private float onStayTime = 0.0f;
    private bool isLightOn = false;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other != null && other.CompareTag("Firefly") && !isLightOn)
        {
            onStayTime += Time.deltaTime;
        
            if (onStayTime > lightUpTime)
            {
                Debug.Log("Firefly detected, lighting up the jar");
                Light light = GetComponentInChildren<Light>();
            
                if (light != null)
                {
                    light.enabled = true;
                    isLightOn = true;
                }
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

