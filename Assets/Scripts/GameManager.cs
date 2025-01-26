using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float levelTimeLimit = 60f; // Time limit in seconds
    [SerializeField] private UIManager uiManager;

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
        DontDestroyOnLoad(gameObject);

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

            if (currentTime >= levelTimeLimit)
            {
                LevelFailed();
            }
        }
    }

    public void StartLevel()
    {
        currentTime = 0f;
        isGameActive = true;
        uiManager.ShowGameUI();
    }

    public void LevelComplete()
    {
        isGameActive = false;

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
        uiManager.ShowFailureUI();
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
