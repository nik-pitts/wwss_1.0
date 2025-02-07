using UnityEngine;

public class ShootingStarMove : MonoBehaviour
{
    public GameObject player;
    public Animator starAnimation;
    private CameraLook cameraLook; 

    private const float StarSpeedMultiplier = 1.5f;
    private const float MaxDistance = 10f;
    private const float Acceleration = 10f; 
    private const float Deceleration = 15f; 
    private const float CatchUpMultiplier = 2.0f;
    private const float DirectionSmoothFactor = 0.01f;
    

    private GamepadControl playerController;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private Vector3 smoothedDirection;

    private float noiseOffset;
    
    private void Start()
    {
        playerController = player.GetComponent<GamepadControl>();
        cameraLook = FindObjectOfType<CameraLook>();
        targetPosition = player.transform.position;
        noiseOffset = Random.Range(0f, 100f);
        smoothedDirection = player.transform.forward;
    }

    private void Update()
    {
        Vector3 playerForward = cameraLook.transform.forward;
        Vector3 playerRight = cameraLook.transform.right;

        playerForward.y = 0;
        playerRight.y = 0;
        playerForward.Normalize();
        playerRight.Normalize();

        Vector3 rawDirection = (playerForward * playerController.currentMovement.y + playerRight * playerController.currentMovement.x).normalized;

        if (rawDirection != Vector3.zero)
        {
            smoothedDirection = Vector3.Slerp(smoothedDirection, rawDirection, DirectionSmoothFactor);
        }

        float playerSpeed = playerController.moveSpeed;

        if (playerController.currentMovement != Vector2.zero)
        {
            float noise = Mathf.PerlinNoise(Time.time * 0.5f + noiseOffset, 0f) * 2f - 1f;
            Vector3 deviation = new Vector3(noise * 2.0f, 0, noise * 2.0f);

            Vector3 idealPosition = player.transform.position + (smoothedDirection * MaxDistance) + deviation;
            idealPosition = GetTerrainPos(idealPosition.x, idealPosition.z); 

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float speedMultiplier = (distanceToPlayer > MaxDistance) ? CatchUpMultiplier : StarSpeedMultiplier;

            targetPosition = Vector3.Lerp(targetPosition, idealPosition, Time.deltaTime * Acceleration);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.2f, playerSpeed * speedMultiplier);

            if (velocity.sqrMagnitude > 0.01f) // Prevents jittering
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }

            starAnimation.ResetTrigger("Sit");
            starAnimation.SetTrigger("Jog");
        }

        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * Deceleration);
            transform.position += velocity * Time.deltaTime;

            starAnimation.ResetTrigger("Jog");
            starAnimation.SetTrigger("Sit");
        }
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