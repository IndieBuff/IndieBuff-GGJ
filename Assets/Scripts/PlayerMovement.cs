using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Blast Pack Settings")]
    [SerializeField] private float minBlastForce = 5f;
    [SerializeField] private float maxBlastForce = 20f;
    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float upwardForceRatio = 0.6f;
    [SerializeField] private float forwardForceRatio = 1f;

    [Header("Dive Settings")]
    [SerializeField] private float diveForce = 20f;
    [SerializeField] private float jumpCooldown = 1.5f;

    private Rigidbody rb;
    private Camera mainCamera;
    private float horizontalInput;
    private float verticalInput;
    private float chargeStartTime;
    private bool isCharging;
    private float lastJumpTime;
    private bool isDiving;



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

        rb.AddForce(forwardForce + upwardForce, ForceMode.Impulse);
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

        // Apply movement
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement);

        // Apply dive force if diving
        if (isDiving)
        {
            rb.AddForce(Vector3.down * diveForce, ForceMode.Force);
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
}
