using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverScoreText;      // assign in Inspector
    public TMP_Text gameOverHighScoreText;  // optional

    [Header("Scene Transition")]
    public float delayBeforeReturnToMenu = 4f;  // unscaled seconds
    public string menuSceneName = "GameScene";

    private bool isGameOver = false;

    void Awake() => instance = this;

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // read final score from ScoreManager
        int finalScore = (ScoreManager.instance != null) ? ScoreManager.instance.score : 0;

        // show panel + texts
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverScoreText) gameOverScoreText.text ="High Score: " + finalScore.ToString();

        // high score
        int high = PlayerPrefs.GetInt("HighScore", 0);
        if (finalScore > high)
        {
            high = finalScore;
            PlayerPrefs.SetInt("HighScore", high);
            PlayerPrefs.Save();
        }
        if (gameOverHighScoreText) gameOverHighScoreText.text = "High Score: " + high.ToString();

        // pause gameplay so player can see the panel
        Time.timeScale = 0.6f;

        // kick back to menu after real time delay
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeReturnToMenu);
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}




