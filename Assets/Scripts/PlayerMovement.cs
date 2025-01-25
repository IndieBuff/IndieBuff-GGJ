using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 movement;

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
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        movement = cameraRight * horizontalInput + cameraForward * verticalInput;

        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void OnDisable()
    {

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
