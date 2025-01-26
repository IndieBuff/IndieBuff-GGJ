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

    [Header("Particle System Settings")]
    [SerializeField] private ParticleSystem speedParticles;
    [SerializeField] private float minEmissionRate = 0f;
    [SerializeField] private float maxEmissionRate = 50f;
    [SerializeField] private float speedThreshold = 5f; // Speed at which particles start emitting

    [SerializeField] private ParticleSystem leftHandBubbles;
    [SerializeField] private ParticleSystem rightHandBubbles;

    [Header("Animation Speed Settings")]
    [SerializeField] private float minAnimationSpeed = 0.5f;
    [SerializeField] private float maxAnimationSpeed = 2f;
    [SerializeField] private float speedThresholdForAnim = 5f; // Speed at which animations start speeding up

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

    private Animator animator;

    private static readonly int IsFloating = Animator.StringToHash("IsFloating");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsDiving = Animator.StringToHash("IsDiving");
    private static readonly int IsRolling = Animator.StringToHash("IsRolling");

    // Add these new fields
    private bool wasJustDiving = false;
    private bool isRolling = false;
    private float rollDuration = 0.2f; // Adjust this to match your roll animation length
    private float rollStartTime;



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
        animator = GetComponent<Animator>(); 

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
        UpdateParticleEmission();
        UpdateAnimationSpeed();
        HandleBubbleParticles();
    }

    private void CaptureInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void UpdateParticleEmission()
    {
        if (speedParticles == null) return;

        var emission = speedParticles.emission;
        float currentSpeed = GetCurrentSpeed();
        
        // Calculate emission rate based on speed
        float normalizedSpeed = Mathf.Clamp01((currentSpeed - speedThreshold) / (maxSpeed - speedThreshold));
        float newEmissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, normalizedSpeed);
        
        // Update the emission rate
        var rate = emission.rateOverTime;
        rate.constant = newEmissionRate;
        emission.rateOverTime = rate;
    }

    private void HandleBubbleParticles()
    {
        if (leftHandBubbles == null || rightHandBubbles == null) return;

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        bool movingRight = horizontalInput > 0;
        bool movingLeft = horizontalInput < 0;
        
        // Handle right hand bubbles
        if (!isGrounded && movingLeft)  // Moving left, right hand shoots bubbles
        {
            if (!rightHandBubbles.isPlaying)
            {
                rightHandBubbles.Play();
            }
        }
        else
        {
            if (rightHandBubbles.isPlaying)
            {
                rightHandBubbles.Stop();
            }
        }

        // Handle left hand bubbles
        if (!isGrounded && movingRight)  // Moving right, left hand shoots bubbles
        {
            if (!leftHandBubbles.isPlaying)
            {
                leftHandBubbles.Play();
            }
        }
        else
        {
            if (leftHandBubbles.isPlaying)
            {
                leftHandBubbles.Stop();
            }
        }
    }

    // Add this method to update animation speed
    private void UpdateAnimationSpeed()
    {
        if (animator == null) return;

        // Calculate normalized speed (0 to 1) based on current speed
        float normalizedSpeed = Mathf.Clamp01((currentSpeed - speedThresholdForAnim) / (maxSpeed - speedThresholdForAnim));
        
        // Lerp between min and max animation speeds
        float targetAnimSpeed = Mathf.Lerp(minAnimationSpeed, maxAnimationSpeed, normalizedSpeed);
        
        // Set single Speed parameter for all animations
        animator.SetFloat("Speed", targetAnimSpeed);
    }


    private void HandleGroundedState()
    {
        Vector3 rayStart = transform.position;
        float scaledDistance = groundCheckDistance * transform.localScale.y;
        
        // Simple debug line in red
        Debug.DrawLine(rayStart, rayStart + (Vector3.down * scaledDistance), Color.red);
        
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Check if we just landed from a dive
        if (isGrounded && wasJustDiving && !isRolling)
        {
            StartRoll();
        }

        wasJustDiving = isDiving;

        isDiving = !isGrounded && verticalInput < -0.5f;
        
        animator.SetBool(IsFloating, !isGrounded && !isDiving);
        animator.SetBool(IsRunning, isGrounded && !isRolling);
        animator.SetBool(IsDiving, isDiving);
        animator.SetBool(IsRolling, isRolling);

        // Check if roll is complete
        if (isRolling && Time.time - rollStartTime >= rollDuration)
        {
            EndRoll();
        }
    }

    private void StartRoll()
    {
        isRolling = true;
        rollStartTime = Time.time;
        // maintain some forward momentum during roll
        //currentSpeed = Mathf.Max(currentSpeed * 0.7f, minSpeedForDive);
    }

    private void EndRoll()
    {
        isRolling = false;
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
        // If rolling, maintain forward momentum but don't allow new input
        if (isRolling)
        {
            /*Vector3 rollVelocity = currentVelocityDir * currentSpeed;
            rollVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = rollVelocity;*/
            return;
        }

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
