using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject failureUI;

    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Victory UI")]
    [SerializeField] private TextMeshProUGUI completionTimeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;

    private void Start()
    {
        retryButton.onClick.AddListener(() => GameManager.Instance.RetryLevel());
    }

    public void ShowGameUI()
    {
        gameUI.SetActive(true);
        victoryUI.SetActive(false);
        failureUI.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }

    public void ShowVictoryUI(float completionTime, float bestTime)
    {
        gameUI.SetActive(false);
        victoryUI.SetActive(true);
        failureUI.SetActive(false);
        retryButton.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        completionTimeText.text = $"Your Time: {FormatTime(completionTime)}";
        bestTimeText.text = $"Best Time: {FormatTime(bestTime)}";
    }

    public void ShowFailureUI()
    {
        gameUI.SetActive(false);
        victoryUI.SetActive(false);
        failureUI.SetActive(true);
        retryButton.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdateTimer(float currentTime)
    {
        timerText.text = FormatTime(currentTime);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
