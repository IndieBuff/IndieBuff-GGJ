using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float speedDecayRate = 0.1f;
    [SerializeField] private float accelerationCurveExponent = 2f;
    [SerializeField] private float groundSpeedDecayMultiplier = 0.2f; // Lower value = less speed loss on ground
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;


    [Header("Blast Pack Settings")]
    [SerializeField] private float minBlastForce = 10f;
    [SerializeField] private float maxBlastForce = 25f;

    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float upwardForceRatio = 0.6f;
    [SerializeField] private float forwardForceRatio = 1f;

    [Header("Dive Settings")]
    [SerializeField] private float diveForce = 30f;
    [SerializeField] private float jumpCooldown = 1.5f;
    [SerializeField] private float minSpeedForDive = 5f;

    private Rigidbody rb;
    private Camera mainCamera;
    private float horizontalInput;
    private float verticalInput;
    private float chargeStartTime;
    private bool isCharging;
    private float lastJumpTime;
    private bool isDiving;
    private float currentSpeed;
    private Vector3 currentVelocityDir;




    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing!");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            enabled = false;
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Capture input in Update for responsiveness
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Handle diving
        isDiving = verticalInput < -0.5f;

        // Handle blast pack charging if cooldown has elapsed

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCharging();
        }

        // Auto-launch if held too long
        if (isCharging && Time.time - chargeStartTime >= maxChargeTime)
        {
            LaunchPlayer(maxChargeTime);
        }
        // Launch when space is released
        else if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;
            LaunchPlayer(chargeTime);
        }
    }

    private void StartCharging()
    {
        // Check cooldown before allowing a new jump
        if (Time.time - lastJumpTime < jumpCooldown)
        {
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;
    }


    private void LaunchPlayer(float chargeTime)
    {
        isCharging = false;

        // Calculate force based on charge time
        float normalizedCharge = Mathf.Clamp01(chargeTime / maxChargeTime);
        float totalForce = Mathf.Lerp(minBlastForce, maxBlastForce, normalizedCharge);

        // Calculate launch direction

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;


        Vector3 forwardForce = cameraForward * (totalForce * forwardForceRatio);
        Vector3 upwardForce = Vector3.up * (totalForce * upwardForceRatio);

        // Calculate boost based on current speed
        float speedRatio = currentSpeed / maxSpeed;
        float boostMultiplier = 1f + (speedRatio * 0.5f); // Up to 50% extra force at max speed

        rb.AddForce((forwardForce + upwardForce) * boostMultiplier, ForceMode.Impulse);
        lastJumpTime = Time.time;


    }


    private void FixedUpdate()
    {
        // Get camera directions each physics step to handle camera movement correctly
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Flatten directions to horizontal plane
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Create movement vector (forward + lateral movement)
        Vector3 movement = cameraForward + (cameraRight * horizontalInput * 0.5f);

        // Normalize to prevent diagonal speed boost
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        // Calculate current speed and direction
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        currentSpeed = horizontalVelocity.magnitude;
        currentVelocityDir = currentSpeed > 0.1f ? horizontalVelocity.normalized : movement.normalized;

        // Check if we're on the ground
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Only decay speed when in air, and decay much slower when on ground
        float actualDecayRate = isGrounded ? speedDecayRate * groundSpeedDecayMultiplier : speedDecayRate;
        currentSpeed = Mathf.Max(0, currentSpeed - (actualDecayRate * Time.fixedDeltaTime));


        // Calculate acceleration factor (gets smaller as we approach max speed)
        float speedRatio = currentSpeed / maxSpeed;
        float accelerationFactor = 1f - Mathf.Pow(speedRatio, accelerationCurveExponent);

        // Apply movement based on current speed and new input
        Vector3 targetVelocity = currentVelocityDir * currentSpeed;

        // Blend between current direction and input direction
        if (movement.magnitude > 0.1f)
        {
            targetVelocity = Vector3.Lerp(targetVelocity, movement * maxSpeed, accelerationFactor * Time.fixedDeltaTime);
        }

        // Maintain vertical velocity
        targetVelocity.y = rb.linearVelocity.y;

        // Apply movement
        rb.linearVelocity = targetVelocity;

        // Apply dive force if diving and moving fast enough
        if (isDiving && currentSpeed > minSpeedForDive)
        {
            rb.AddForce(Vector3.down * diveForce * accelerationFactor, ForceMode.Force);
            // Convert downward momentum into forward momentum and increase speed
            rb.AddForce(currentVelocityDir * (diveForce * 1.5f) * accelerationFactor, ForceMode.Force);
            // Increase current speed during dive
            currentSpeed = Mathf.Min(currentSpeed + (diveForce * Time.fixedDeltaTime), maxSpeed * 1.5f);
        }




        // Rotate player to face camera's forward direction
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

}
