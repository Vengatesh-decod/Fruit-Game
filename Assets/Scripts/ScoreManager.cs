using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Animation Settings")]
    public float comboScale = 0.8f;
    public float animationDuration = 0.3f;

    private Vector3 originalScale;

    void Awake()
    {
        instance = this;
        if (scoreText != null)
            originalScale = scoreText.rectTransform.localScale;
    }

    /// <summary>
    /// Call this normally: AddScore(1); or AddScore(5, true);
    /// </summary>
    public void AddScore(int amount, bool isCombo = false)
    {
        score += amount;
        scoreText.text = "Score: " + score;

        if (isCombo)
        {
            AnimateComboText();
        }
    }

    private void AnimateComboText()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleEffect());
    }

    private IEnumerator ScaleEffect()
    {
        RectTransform rect = scoreText.rectTransform;

        Vector3 targetScale = originalScale * comboScale;
        float t = 0;

        // Scale Up
        while (t < animationDuration / 2f)
        {
            rect.localScale = Vector3.Lerp(originalScale, targetScale, t / (animationDuration / 2f));
            t += Time.deltaTime;
            yield return null;
        }

        rect.localScale = targetScale;

        // Scale Down
        t = 0;
        while (t < animationDuration / 2f)
        {
            rect.localScale = Vector3.Lerp(targetScale, originalScale, t / (animationDuration / 2f));
            t += Time.deltaTime;
            yield return null;
        }

        rect.localScale = originalScale;
    }
}


