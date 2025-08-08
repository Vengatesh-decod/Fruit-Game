using UnityEngine;
using UnityEngine.UI;
public class AudioToggleUI : MonoBehaviour
{
    [Header("Music Button")]
    public Button musicToggleButton;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    [Header("SFX Button")]
    public Button sfxToggleButton;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;
    private Image musicButtonImage;
    private Image sfxButtonImage;
    void Start()
    {
        musicButtonImage = musicToggleButton.GetComponent<Image>();
        sfxButtonImage = sfxToggleButton.GetComponent<Image>();
        musicToggleButton.onClick.AddListener(ToggleMusic);
        sfxToggleButton.onClick.AddListener(ToggleSFX);
        UpdateButtonImages();
    }
    void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
        AudioManager.Instance.PlaySFX("click",false);
        UpdateButtonImages();
    }
    void ToggleSFX()
    {
        AudioManager.Instance.ToggleSFX();
        UpdateButtonImages();
    }
    void UpdateButtonImages()
    {
        if (musicButtonImage != null)
            musicButtonImage.sprite = AudioManager.Instance.isMusicOn ? musicOnSprite : musicOffSprite;
        if (sfxButtonImage != null)
            sfxButtonImage.sprite = AudioManager.Instance.isSFXOn ? sfxOnSprite : sfxOffSprite;
    }
}
