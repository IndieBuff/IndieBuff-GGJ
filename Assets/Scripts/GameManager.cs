using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float levelTimeLimit = 60f; // Time limit in seconds
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Rigidbody playerRigidbody;

    private float currentTime;
    private float bestTime = float.MaxValue;
    private bool isGameActive;
    private static GameManager instance;

    public static GameManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Load best time from PlayerPrefs
        bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
    }

    private void Start()
    {
        StartLevel();
    }

    private void Update()
    {
        if (isGameActive)
        {
            currentTime += Time.deltaTime;
            uiManager.UpdateTimer(currentTime);

            UpdatePlayerSpeed();
            uiManager.UpdateTimeLeft(levelTimeLimit - currentTime);

            if (currentTime >= levelTimeLimit)
            {
                LevelFailed();
            }
        }
    }

    private void UpdatePlayerSpeed()
    {
        float speed = playerRigidbody.linearVelocity.magnitude;
        uiManager.UpdateSpeed(speed);
    }

    public void StartLevel()
    {
        currentTime = 0f;
        Time.timeScale = 1f;
        isGameActive = true;
        uiManager.ShowGameUI();
    }

    public void LevelComplete()
    {
        isGameActive = false;
        Time.timeScale = 0f;

        if (currentTime < bestTime)
        {
            bestTime = currentTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
        }

        uiManager.ShowVictoryUI(currentTime, bestTime);
    }

    public void LevelFailed()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        uiManager.ShowFailureUI();
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
