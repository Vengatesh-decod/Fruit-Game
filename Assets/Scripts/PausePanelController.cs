using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PausePanelController : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button closeButton;
    public Button menuButton;
    void Start()
    {
        // start hidden
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() =>
            {
                OpenPause();
                PlayClick();
            });
        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                ClosePause();
                PlayClick();
            });
        if (menuButton != null)
            menuButton.onClick.AddListener(() =>
            {
                ReturnToMenu();
                PlayClick();
            });
    }
    void OpenPause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }
    void ClosePause()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
    void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("click",false);
    }
}
