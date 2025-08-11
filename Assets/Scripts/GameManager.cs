using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverScoreText;      // Assign in Inspector
    public TMP_Text gameOverHighScoreText;  // Assign in Inspector

    [Header("Scene Transition")]
    public float delayBeforeReturnToMenu = 4f;  // unscaled seconds
    public string menuSceneName = "GameScene";

    private bool isGameOver = false;
    private int highScore; // cached high score

    void Awake()
    {
        instance = this;
        // Load saved high score when game starts
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void OnEnable()
    {
        // If MissTracker exists, listen for game-over event (after 5 misses)
        if (MissTracker.Instance != null)
            MissTracker.Instance.OnGameOver += GameOver;
    }

    void OnDisable()
    {
        if (MissTracker.Instance != null)
            MissTracker.Instance.OnGameOver -= GameOver;
    }

    void Start()
    {
        // Reset miss counter at the start of gameplay
        if (MissTracker.Instance != null)
            MissTracker.Instance.ResetMisses();

        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = "Best : " + highScore.ToString();

        // Ensure normal time scale at start
        Time.timeScale = 1f;

        // Make sure panel starts hidden
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // ✅ Stop spawner & clear existing fruits
        FruitSpawner spawner = FindObjectOfType<FruitSpawner>();
        if (spawner != null)
            spawner.StopSpawning();

        // Final score
        int finalScore = (ScoreManager.instance != null) ? ScoreManager.instance.score : 0;

        // High score check
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Show Game Over UI
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverScoreText) gameOverScoreText.text = "Score : " + finalScore.ToString();
        if (gameOverHighScoreText) gameOverHighScoreText.text = "Best : " + highScore.ToString();

        // Slow motion (use unscaled wait for transitions)
        Time.timeScale = 0.6f;

        // Return to menu after delay (unscaled)
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeReturnToMenu);
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // Optional: hook this to a "Restart" button on the Game Over panel
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }
}
