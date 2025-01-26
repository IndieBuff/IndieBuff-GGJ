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

    private float originalStartSpeed;
    private float originalStartSize;
    private Vector3 originalShapeScale;
    void Start()
    {
        bubblePrefab.GetComponent<Renderer>().enabled = false;

        if (popParticleEffect == null)
        {
            popParticleEffect = GetComponentInChildren<ParticleSystem>();
        }

        if (popParticleEffect != null)
        {
            var main = popParticleEffect.main;
            var shape = popParticleEffect.shape;

            originalStartSpeed = main.startSpeed.constant;
            originalStartSize = main.startSize.constant;
            originalShapeScale = shape.scale;
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
            var main = popParticleEffect.main;
            var shape = popParticleEffect.shape;

            main.startSpeed = originalStartSpeed;
            main.startSize = originalStartSize;
            shape.scale = originalShapeScale;

            float bubbleScale = transform.localScale.x / minBubbleScale * 0.5f;
            Debug.Log(bubbleScale);
            main.startSpeed = originalStartSpeed * bubbleScale;
            main.startSize = originalStartSize * bubbleScale;
            shape.scale = new Vector3(
                originalShapeScale.x * bubbleScale,
                originalShapeScale.y * bubbleScale,
                originalShapeScale.z * bubbleScale
            );

            popParticleEffect.Play();
        }

        bubblePrefab.GetComponent<Renderer>().enabled = false;
        transform.localScale = minBubbleScale * Vector3.one;

        isCharging = false;
        lastJumpTime = Time.time;
    }
}
