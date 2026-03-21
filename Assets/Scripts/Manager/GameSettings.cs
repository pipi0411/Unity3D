using UnityEngine;

public static class GameSettings
{
    private const string MouseSensitivityKey = "settings.mouseSensitivity";
    private const string MasterVolumeKey = "settings.masterVolume";
    private const string ResolutionWidthKey = "settings.resolutionWidth";
    private const string ResolutionHeightKey = "settings.resolutionHeight";
    private const string FullscreenKey = "settings.fullscreen";

    private const float DefaultMouseSensitivity = 1f;
    private const float DefaultMasterVolume = 1f;
    private const bool DefaultFullscreen = false;

    public static float MouseSensitivity { get; private set; } = DefaultMouseSensitivity;
    public static float MasterVolume { get; private set; } = DefaultMasterVolume;
    public static bool IsFullscreen { get; private set; } = DefaultFullscreen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnBoot()
    {
        Load();
        ApplyAudio();
        ApplyResolution();
    }

    public static void Load()
    {
        MouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity), 0.1f, 5f);
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
        IsFullscreen = PlayerPrefs.GetInt(FullscreenKey, DefaultFullscreen ? 1 : 0) == 1;
    }

    public static void SetMouseSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, 0.1f, 5f);
        PlayerPrefs.SetFloat(MouseSensitivityKey, MouseSensitivity);
        PlayerPrefs.Save();
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
        ApplyAudio();
    }

    public static bool TryGetSavedResolution(out int width, out int height)
    {
        if (!PlayerPrefs.HasKey(ResolutionWidthKey) || !PlayerPrefs.HasKey(ResolutionHeightKey))
        {
            width = 0;
            height = 0;
            return false;
        }

        width = PlayerPrefs.GetInt(ResolutionWidthKey);
        height = PlayerPrefs.GetInt(ResolutionHeightKey);
        return width > 0 && height > 0;
    }

    public static void SetResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        PlayerPrefs.SetInt(ResolutionWidthKey, width);
        PlayerPrefs.SetInt(ResolutionHeightKey, height);
        PlayerPrefs.Save();

        Screen.SetResolution(width, height, GetPreferredScreenMode());
    }

    public static void SetFullscreen(bool fullscreen)
    {
        IsFullscreen = fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, IsFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        int width;
        int height;
        if (!TryGetSavedResolution(out width, out height))
        {
            width = Screen.width;
            height = Screen.height;
        }

        Screen.SetResolution(width, height, GetPreferredScreenMode());
    }

    private static void ApplyAudio()
    {
        AudioListener.volume = MasterVolume;
    }

    private static void ApplyResolution()
    {
        if (TryGetSavedResolution(out int width, out int height))
        {
            Screen.SetResolution(width, height, GetPreferredScreenMode());
        }
        else
        {
            Screen.fullScreenMode = GetPreferredScreenMode();
            Screen.fullScreen = IsFullscreen;
        }
    }

    private static FullScreenMode GetPreferredScreenMode()
    {
        return IsFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }
}
