using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Music Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.8f;
    [SerializeField] private bool loopMusic = true;
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

    [Header("Optional SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private AudioSource musicSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureMusicSource();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }

        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    private void EnsureMusicSource()
    {
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = loopMusic;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            PlayMainMenuMusic();
            return;
        }

        PlayGameplayMusic();
    }

    private void PlayMusic(AudioClip targetClip)
    {
        if (musicSource == null || targetClip == null)
        {
            return;
        }

        if (musicSource.clip == targetClip && musicSource.isPlaying)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (fadeDuration <= 0f || !musicSource.isPlaying)
        {
            musicSource.clip = targetClip;
            musicSource.loop = loopMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
            return;
        }

        fadeCoroutine = StartCoroutine(FadeToClip(targetClip));
    }

    private IEnumerator FadeToClip(AudioClip targetClip)
    {
        float startVolume = musicSource.volume;
        float outTime = 0f;

        while (outTime < fadeDuration)
        {
            outTime += Time.unscaledDeltaTime;
            float t = outTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = targetClip;
        musicSource.loop = loopMusic;
        musicSource.Play();

        float inTime = 0f;
        while (inTime < fadeDuration)
        {
            inTime += Time.unscaledDeltaTime;
            float t = inTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        musicSource.volume = musicVolume;
        fadeCoroutine = null;
    }
}
