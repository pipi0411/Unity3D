using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button endButton;
    [SerializeField] private Button tutorButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private SettingsPanelUI settingsPanel;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners(); // Xóa sự kiện cũ nếu có
            startButton.onClick.AddListener(LoadGameScene);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueSavedGame);
            continueButton.interactable = SaveManager.HasSaveFile;
        }

        if (endButton != null)
        {
            endButton.onClick.RemoveAllListeners();
            endButton.onClick.AddListener(QuitGame);
        }

        if (tutorButton != null)
        {
            tutorButton.onClick.RemoveAllListeners();
            tutorButton.onClick.AddListener(LoadTutorialScene);
        }

        if (settingButton != null && settingsPanel != null)
        {
            settingButton.onClick.RemoveAllListeners();
            settingButton.onClick.AddListener(settingsPanel.TogglePanel);
        }
    }

    public void LoadGameScene()  
    {
        SaveManager.DeleteSavedGame();

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()  
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void LoadTutorialScene()
    {
        SceneManager.LoadScene("TutorialScene");   // Tên scene hướng dẫn
    }

    public void ContinueSavedGame()
    {
        SaveManager.LoadSavedGame();
    }

}