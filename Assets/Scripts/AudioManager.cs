using UnityEngine;
using System.Collections.Generic;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSourcePrefab;
    [Header("Music")]
    public AudioClip backgroundMusic;
    [Header("SFX Clips (Name, Clip)")]
    public List<SFXEntry> sfxClips;
    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    private Dictionary<AudioClip, AudioSource> playingSFX = new Dictionary<AudioClip, AudioSource>();
    public bool isMusicOn = true;
    public bool isSFXOn = true;
    [System.Serializable]
    public class SFXEntry
    {
        public string name;
        public AudioClip clip;
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // Load preferences
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;
        // Set music mute state
        if (musicSource != null)
        {
            musicSource.mute = !isMusicOn;
        }
        // Add all SFX entries to dictionary
        foreach (var entry in sfxClips)
        {
            if (!sfxLibrary.ContainsKey(entry.name) && entry.clip != null)
            {
                sfxLibrary.Add(entry.name, entry.clip);
            }
        }
        // Start background music
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (musicSource != null)
            musicSource.mute = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        PlayerPrefs.SetInt("SFXOn", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void PlaySFX(AudioClip clip, bool loop = false)
    {
        if (clip == null || !isSFXOn) return;
        if (loop && playingSFX.ContainsKey(clip)) return;
        AudioSource newSource = Instantiate(sfxSourcePrefab, transform);
        newSource.clip = clip;
        newSource.loop = loop;
        newSource.Play();
        if (loop)
            playingSFX[clip] = newSource;
        else
            Destroy(newSource.gameObject, clip.length);
    }
    public void PlaySFX(string clipName, bool loop = false)
    {
        if (sfxLibrary.ContainsKey(clipName))
        {
            PlaySFX(sfxLibrary[clipName], loop);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{clipName}' not found!");
        }
    }
    public void StopSFX(AudioClip clip)
    {
        if (playingSFX.ContainsKey(clip))
        {
            AudioSource source = playingSFX[clip];
            source.Stop();
            Destroy(source.gameObject);
            playingSFX.Remove(clip);
        }
    }
    public void StopSFX(string clipName)
    {
        if (sfxLibrary.ContainsKey(clipName))
        {
            StopSFX(sfxLibrary[clipName]);
        }
    }
    public void StopAllSFX()
    {
        foreach (var kvp in playingSFX)
        {
            kvp.Value.Stop();
            Destroy(kvp.Value.gameObject);
        }
        playingSFX.Clear();
    }
    public void PlayMusic()
    {
        if (!musicSource.isPlaying)
            musicSource.Play();
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
