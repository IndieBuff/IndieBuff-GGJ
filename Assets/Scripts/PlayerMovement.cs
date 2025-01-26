using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float speedDecayRate = 0.1f;
    [SerializeField] private float accelerationCurveExponent = 2f;
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

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

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
    private RaycastHit slopeHit;

    private void Start()
    {
        InitializeComponents();
        ConfigureCursor();
        gameObject.AddComponent<CheckpointManager>();
    }


    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (rb == null || mainCamera == null)
        {
            Debug.LogError("Required components missing!");
            enabled = false;
        }
    }

    private void ConfigureCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        CaptureInput();
        HandleGroundedState();
        HandleBlastPackCharging();
    }

    private void CaptureInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void HandleGroundedState()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        isDiving = !isGrounded && verticalInput < -0.5f;
    }

    private void HandleBlastPackCharging()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCharging();
        }

        if (isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;
            if (chargeTime >= maxChargeTime)
            {
                LaunchPlayer(maxChargeTime);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                LaunchPlayer(chargeTime);
            }
        }
    }

    private void StartCharging()
    {
        if (Time.time - lastJumpTime >= jumpCooldown)
        {
            isCharging = true;
            chargeStartTime = Time.time;
        }
    }

    private void LaunchPlayer(float chargeTime)
    {
        isCharging = false;
        float normalizedCharge = Mathf.Clamp01(chargeTime / maxChargeTime);
        float totalForce = Mathf.Lerp(minBlastForce, maxBlastForce, normalizedCharge);

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 forwardForce = cameraForward * (totalForce * forwardForceRatio);
        Vector3 upwardForce = Vector3.up * (totalForce * upwardForceRatio);

        float speedRatio = currentSpeed / maxSpeed;
        float boostMultiplier = 1f + (speedRatio * 0.5f);

        rb.AddForce((forwardForce + upwardForce) * boostMultiplier, ForceMode.Impulse);
        lastJumpTime = Time.time;
    }

    private void FixedUpdate()
    {
        Vector3 movementDirection = CalculateMovementDirection();
        UpdateCurrentSpeedAndDirection(movementDirection);
        ApplyMovement(movementDirection);
        HandleDiving();
        RotatePlayer();
    }

    private Vector3 CalculateMovementDirection()
    {
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        Vector3 movement = isGrounded
            ? cameraForward
            : (cameraForward + (cameraRight * horizontalInput * 0.5f));

        return movement.magnitude > 1f ? movement.normalized : movement;
    }

    private void UpdateCurrentSpeedAndDirection(Vector3 movement)
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        currentSpeed = horizontalVelocity.magnitude;
        currentVelocityDir = currentSpeed > 0.1f ? horizontalVelocity.normalized : movement.normalized;

        currentSpeed = Mathf.Max(0, currentSpeed - (speedDecayRate * Time.fixedDeltaTime));
    }

    private void ApplyMovement(Vector3 movement)
    {
        float speedRatio = currentSpeed / maxSpeed;
        float accelerationFactor = 1f - Mathf.Pow(speedRatio, accelerationCurveExponent);

        Vector3 targetVelocity = currentVelocityDir * currentSpeed;

        if (movement.magnitude > 0.1f)
        {
            targetVelocity = Vector3.Lerp(targetVelocity, movement * maxSpeed, accelerationFactor * Time.fixedDeltaTime);
        }

        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    private void HandleDiving()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (!isGrounded && isDiving && currentSpeed > minSpeedForDive)
        {
            rb.AddForce(Vector3.down * diveForce * 5f, ForceMode.Force);

            float diveSpeedMultiplier = 3f;
            rb.AddForce(currentVelocityDir * (diveForce * diveSpeedMultiplier), ForceMode.Force);

            float diveMaxSpeed = maxSpeed * 1.25f;
            currentSpeed = Mathf.Min(currentSpeed + (diveForce * diveSpeedMultiplier * Time.fixedDeltaTime), diveMaxSpeed);

            Vector3 targetVelocity = currentVelocityDir * currentSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
    }

    private void RotatePlayer()
    {
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Kept optional methods for potential external use
    public float GetCurrentSpeed() => currentSpeed;

    private bool OnSlope()
    {
        return Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundCheckDistance)
            && Vector3.Angle(Vector3.up, slopeHit.normal) < maxSlopeAngle;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(currentVelocityDir, slopeHit.normal).normalized;
    }
}
