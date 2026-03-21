using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    [Header("Controls")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Text mouseSensitivityValueText;
    [SerializeField] private TMP_Text masterVolumeValueText;

    private readonly List<Vector2Int> resolutionOptions = new List<Vector2Int>();
    private bool isInitializing;
    private const float AspectTolerance = 0.02f;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        BuildResolutionDropdown();
        LoadSettingsToUI();
        BindUIEvents();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
        }
    }

    public void OpenPanel()
    {
        LoadSettingsToUI();
        panelRoot.SetActive(true);
    }

    public void ClosePanel()
    {
        panelRoot.SetActive(false);
    }

    public void TogglePanel()
    {
        if (panelRoot.activeSelf)
        {
            ClosePanel();
            return;
        }

        OpenPanel();
    }

    private void BindUIEvents()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }
    }

    private void LoadSettingsToUI()
    {
        isInitializing = true;

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = GameSettings.MouseSensitivity;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = GameSettings.MasterVolume;
        }

        if (resolutionDropdown != null && resolutionOptions.Count > 0)
        {
            int selectedIndex = FindCurrentResolutionIndex();
            resolutionDropdown.SetValueWithoutNotify(selectedIndex);
            resolutionDropdown.RefreshShownValue();
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.IsFullscreen);
        }

        RefreshValueTexts();

        isInitializing = false;
    }

    private void RefreshValueTexts()
    {
        if (mouseSensitivityValueText != null)
        {
            float value = mouseSensitivitySlider != null ? mouseSensitivitySlider.value : GameSettings.MouseSensitivity;
            mouseSensitivityValueText.text = value.ToString("0.00");
        }

        if (masterVolumeValueText != null)
        {
            float value = masterVolumeSlider != null ? masterVolumeSlider.value : GameSettings.MasterVolume;
            masterVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        resolutionOptions.Clear();
        resolutionDropdown.ClearOptions();

        Resolution[] allResolutions = Screen.resolutions;
        var optionLabels = new List<string>();
        float targetAspect = (float)Screen.currentResolution.width / Screen.currentResolution.height;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            Vector2Int current = new Vector2Int(allResolutions[i].width, allResolutions[i].height);
            if (resolutionOptions.Contains(current))
            {
                continue;
            }

            float currentAspect = (float)current.x / current.y;
            if (Mathf.Abs(currentAspect - targetAspect) > AspectTolerance)
            {
                continue;
            }

            resolutionOptions.Add(current);
            optionLabels.Add(current.x + " x " + current.y);
        }

        if (resolutionOptions.Count == 0)
        {
            AddFallbackResolution(optionLabels, 1280, 720);
            AddFallbackResolution(optionLabels, 1600, 900);
            AddFallbackResolution(optionLabels, 1920, 1080);
        }

        resolutionDropdown.AddOptions(optionLabels);
    }

    private void AddFallbackResolution(List<string> labels, int width, int height)
    {
        Vector2Int fallback = new Vector2Int(width, height);
        if (resolutionOptions.Contains(fallback))
        {
            return;
        }

        resolutionOptions.Add(fallback);
        labels.Add(fallback.x + " x " + fallback.y);
    }

    private int FindCurrentResolutionIndex()
    {
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        if (GameSettings.TryGetSavedResolution(out int savedWidth, out int savedHeight))
        {
            width = savedWidth;
            height = savedHeight;
        }

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            if (resolutionOptions[i].x == width && resolutionOptions[i].y == height)
            {
                return i;
            }
        }

        return resolutionOptions.Count - 1;
    }

    private void OnMouseSensitivityChanged(float value)
    {
        RefreshValueTexts();

        if (isInitializing)
        {
            return;
        }

        GameSettings.SetMouseSensitivity(value);
    }

    private void OnMasterVolumeChanged(float value)
    {
        RefreshValueTexts();

        if (isInitializing)
        {
            return;
        }

        GameSettings.SetMasterVolume(value);
    }

    private void OnResolutionChanged(int index)
    {
        if (isInitializing)
        {
            return;
        }

        if (index < 0 || index >= resolutionOptions.Count)
        {
            return;
        }

        Vector2Int selected = resolutionOptions[index];
        GameSettings.SetResolution(selected.x, selected.y);
    }

    private void OnFullscreenToggleChanged(bool isOn)
    {
        if (isInitializing)
        {
            return;
        }

        GameSettings.SetFullscreen(isOn);
    }
}
