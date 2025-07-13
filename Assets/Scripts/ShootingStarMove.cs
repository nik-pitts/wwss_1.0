using Oculus.Interaction.Locomotion;
using UnityEngine;

public class ShootingStarMove : MonoBehaviour
{
    public GameObject player;
    public Animator starAnimation;
    private CameraLook cameraLook;

    private const float StarSpeedMultiplier = 1.5f;
    private const float RunStarSpeedMultiplier = 3.0f;
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

    // fetching behavior related variables
    private GameObject thrownBall;
    private GameObject carriedBall;
    public GameObject ballPoint;
    private bool isAiming = false;
    private bool isChasingBall = false;
    private bool isReturningToPlayer = false;

    [SerializeField] GameObject ballPrefab;

    [SerializeField] private GameObject chatGptManagerGameObj;
    private ChatGPTManager chatGptManager;
    private string lastActivity = "";
    private string currentActivity = "";

    // Water avoidance related variables
    private bool isAvoidingWater = false;
    private Vector3 avoidanceDirection;
    private float avoidanceTimer = 0f;
    private const float AvoidanceTime = 1.0f;
    private const float AvoidanceSpeed = 5.0f;

    private void Start()
    {
        playerController = player.GetComponent<GamepadControl>();
        cameraLook = FindObjectOfType<CameraLook>();
        targetPosition = player.transform.position;
        noiseOffset = Random.Range(0f, 100f);
        smoothedDirection = player.transform.forward;
        chatGptManager = chatGptManagerGameObj.GetComponent<ChatGPTManager>();
    }

    private void Update()
    {

        if (isAvoidingWater)
        {
            HandleWaterAvoidance();
            return;
        }
        // chasing mode
        if (isChasingBall && thrownBall != null)
        {
            MoveTowardsTarget(thrownBall.transform.position, RunStarSpeedMultiplier);
            MoveTowardsTarget(thrownBall.transform.position, RunStarSpeedMultiplier);
            currentActivity = "We are playing fetch!";
            if (Vector3.Distance(transform.position, thrownBall.transform.position) < 1.0f)
            {
                isChasingBall = false;
                starAnimation.ResetTrigger("Idle");
                starAnimation.ResetTrigger("Jog");
                starAnimation.ResetTrigger("Wait");
                starAnimation.SetTrigger("Run");
                PickUpBall();
            }
            return;
        }

        // return to the player
        if (isReturningToPlayer)
        {
            MoveTowardsTarget(player.transform.position, RunStarSpeedMultiplier);

            if (Vector3.Distance(transform.position, player.transform.position) < 5.0f)
            {
                DropBall();
            }
        }

        // aiming mode
        if (isAiming)
        {
            starAnimation.ResetTrigger("Jog");
            starAnimation.ResetTrigger("Run");
            starAnimation.SetTrigger("Wait");
            return;
        }

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
        float starSpeed = playerController.isRunning ? RunStarSpeedMultiplier : StarSpeedMultiplier;


        if (playerController.currentMovement != Vector2.zero)
        {
            float noise = Mathf.PerlinNoise(Time.time * 0.5f + noiseOffset, 0f) * 2f - 1f;
            Vector3 deviation = new Vector3(noise * 2.0f, 0, noise * 2.0f);

            Vector3 idealPosition = player.transform.position + (smoothedDirection * MaxDistance) + deviation;
            idealPosition = GetTerrainPos(idealPosition.x, idealPosition.z);

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float speedMultiplier = (distanceToPlayer > MaxDistance) ? CatchUpMultiplier : starSpeed;

            targetPosition = Vector3.Lerp(targetPosition, idealPosition, Time.deltaTime * Acceleration);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.2f, playerSpeed * speedMultiplier);

            if (velocity.sqrMagnitude > 0.01f) // Prevents jittering
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }
            if (playerController.isRunning)
            {
                starAnimation.ResetTrigger("Idle");
                starAnimation.ResetTrigger("Jog");
                starAnimation.ResetTrigger("Wait");
                starAnimation.SetTrigger("Run");
                currentActivity = "We are running together!";
            }
            else
            {
                starAnimation.ResetTrigger("Idle");
                starAnimation.ResetTrigger("Run");
                starAnimation.ResetTrigger("Wait");
                starAnimation.SetTrigger("Jog");
                currentActivity = "We are leisurely walking together.";
            }
        }
        else
        {
            Debug.Log("Player is not moving.");
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * Deceleration);
            transform.position += velocity * Time.deltaTime;
            starAnimation.ResetTrigger("Run");
            starAnimation.ResetTrigger("Jog");
            starAnimation.SetTrigger("Idle");
        }

        if (currentActivity != lastActivity)
        {
            Debug.Log(currentActivity);
            chatGptManager.NotifyActivityChange(currentActivity);
            lastActivity = currentActivity;
        }
    }

    private void PickUpBall()
    {
        isChasingBall = false;
        if (thrownBall != null)
        {
            Destroy(thrownBall);
            thrownBall = null;
        }

        starAnimation.ResetTrigger("Idle");
        starAnimation.ResetTrigger("Jog");
        starAnimation.ResetTrigger("Wait");
        starAnimation.SetTrigger("Run");

        // return to player
        isReturningToPlayer = true;
    }
    private void DropBall()
    {
        isReturningToPlayer = false;

        // new ball
        carriedBall = Instantiate(ballPrefab, ballPoint.transform.position, Quaternion.identity);
        ballPrefab.GetComponent<Rigidbody>().isKinematic = false;

        // Ball disappears in 2 seconds
        Destroy(carriedBall, 4.0f);

        // Animation
        starAnimation.ResetTrigger("Run");
        starAnimation.SetTrigger("Wait");
    }
    private void MoveTowardsTarget(Vector3 destination, float speedMultiplier)
    {
        // position related
        Vector3 adjustedDestination = GetTerrainPos(destination.x, destination.z);
        targetPosition = Vector3.Lerp(targetPosition, adjustedDestination, Time.deltaTime * Acceleration);
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.2f, speedMultiplier);
        newPosition.y = GetTerrainPos(newPosition.x, newPosition.z).y;
        transform.position = newPosition;

        // face movement direction
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }

        // animation
        starAnimation.ResetTrigger("Idle");
        starAnimation.ResetTrigger("Jog");
        starAnimation.ResetTrigger("Wait");
        starAnimation.SetTrigger("Run");
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

    // Called when the player starts aiming
    public void StartAiming()
    {
        isAiming = true;
    }

    // Called when the player stops aiming
    public void StopAiming()
    {
        isAiming = false;
    }

    // Called when the player throws the ball
    public void ChaseBall(GameObject ball)
    {
        isAiming = false;
        isChasingBall = true;
        thrownBall = ball;
    }

    private void HandleWaterAvoidance()
    {
        avoidanceTimer += Time.deltaTime;

        // Calculate avoidance target
        Vector3 avoidanceTarget = transform.position + avoidanceDirection * AvoidanceSpeed;

        // Move towards the avoidance target and ensure final position is on terrain
        Vector3 newPosition = Vector3.MoveTowards(transform.position, avoidanceTarget, AvoidanceSpeed * Time.deltaTime);
        newPosition = GetTerrainPos(newPosition.x, newPosition.z);
        transform.position = newPosition;

        // 회피 방향으로 회전
        if (avoidanceDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(avoidanceDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }

        // 회피 애니메이션
        starAnimation.ResetTrigger("Idle");
        starAnimation.ResetTrigger("Jog");
        starAnimation.ResetTrigger("Wait");
        starAnimation.SetTrigger("Run");

        // 일정 시간 후 회피 모드 해제
        if (avoidanceTimer >= AvoidanceTime)
        {
            isAvoidingWater = false;
            avoidanceTimer = 0f;
            velocity = Vector3.zero;
            Debug.Log("Water avioidance ended!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isAvoidingWater = true;  // 새로 추가
            avoidanceTimer = 0f;     // 새로 추가

            // Calculate avoidance direction away from water
            Vector3 toWater = (other.transform.position - transform.position).normalized;
            avoidanceDirection = -toWater;
            
            avoidanceDirection.y = 0;
            avoidanceDirection.Normalize();

            Debug.Log("Starting water avoidance!");
        }
    }
}