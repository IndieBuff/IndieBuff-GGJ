using UnityEngine;

public class BubblePack : MonoBehaviour
{

    private float chargeStartTime;
    private bool isCharging;
    private float lastJumpTime;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float maxChargeTime = 1.5f;

    [SerializeField] private float minBubbleScale = 0.2f;
    [SerializeField] private float maxBubbleScale = 1f;

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private ParticleSystem popParticleEffect;

    void Start()
    {
        bubblePrefab.GetComponent<Renderer>().enabled = false;

        if (popParticleEffect == null)
        {
            popParticleEffect = GetComponentInChildren<ParticleSystem>();
        }
    }

    private void Update()
    {
        HandleBlastPackCharging();
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

            float normalizedChargeTime = Mathf.Clamp01(chargeTime / maxChargeTime);
            float bubbleScale = Mathf.Lerp(minBubbleScale, maxBubbleScale, normalizedChargeTime);
            transform.localScale = Vector3.one * bubbleScale;

            if (chargeTime >= maxChargeTime)
            {
                PopBubble(maxChargeTime);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                PopBubble(chargeTime);
            }
        }
    }

    private void StartCharging()
    {
        if (Time.time - lastJumpTime >= jumpCooldown)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            bubblePrefab.GetComponent<Renderer>().enabled = true;
        }
    }

    private void PopBubble(float chargeTime)
    {
        if (Time.time - lastJumpTime < jumpCooldown) return;

        if (popParticleEffect != null)
        {
            popParticleEffect.Play();
        }

        bubblePrefab.GetComponent<Renderer>().enabled = false;
        transform.localScale = minBubbleScale * Vector3.one;
        Debug.Log(chargeTime);

        isCharging = false;
        lastJumpTime = Time.time;
    }
}
