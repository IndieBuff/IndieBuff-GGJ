using UnityEngine;

public class BubbleManController : MonoBehaviour
{
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float maxJumpForce = 1000f;
    [SerializeField] private float upwardForceRatio = 0.6f;
    [SerializeField] private float forwardForceRatio = 0.4f;

    private Rigidbody rb;
    private float chargeStartTime;
    private bool isCharging;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Start charging when Space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCharging();
        }

        // Release charge when Space is released
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ReleaseCharge();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
    }

    private void ReleaseCharge()
    {
        if (!isCharging) return;

        float chargeTime = Mathf.Min(Time.time - chargeStartTime, maxChargeTime);
        float chargeRatio = chargeTime / maxChargeTime;
        float totalForce = maxJumpForce * chargeRatio;

        Vector3 upForce = Vector3.up * (totalForce * upwardForceRatio);
        Vector3 forwardForce = transform.forward * (totalForce * forwardForceRatio);

        rb.AddForce(upForce + forwardForce, ForceMode.Impulse);
        isCharging = false;
    }
}

