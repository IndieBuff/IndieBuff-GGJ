using UnityEngine;
using TMPro;

public class SpeedDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private string displayFormat = "Speed: {0:F1} m/s";

    private void Start()
    {
        if (speedText == null)
        {
            speedText = GetComponent<TextMeshProUGUI>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (speedText == null || playerMovement == null)
        {
            Debug.LogError("SpeedDisplay: Missing required components!");
            enabled = false;
        }
    }

    private void Update()
    {
        float currentSpeed = playerMovement.GetCurrentSpeed();
        speedText.text = string.Format(displayFormat, currentSpeed);
    }
}
