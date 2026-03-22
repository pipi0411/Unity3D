using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private SettingsPanelUI settingsPanel;

    private void Start()
    {
        // Tạm dừng game khi vào pause
        Time.timeScale = 0f;

        // Khôi phục chuột để click được nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(SaveGame);
        }

        if (settingButton != null && settingsPanel != null)
        {
            settingButton.onClick.RemoveAllListeners();
            settingButton.onClick.AddListener(settingsPanel.TogglePanel);
        }
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;

        // Khôi phục chuột (dự phòng)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Đóng scene pause trước, rồi mới bật lại các system đã tắt để tránh trùng EventSystem.
        Scene pauseScene = SceneManager.GetSceneByName("PauseScene");
        if (pauseScene.IsValid() && pauseScene.isLoaded)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync("PauseScene");
            if (unloadOperation != null)
            {
                unloadOperation.completed += _ => PauseInput.RestoreSystemsAfterPause();
                return;
            }
        }

        PauseInput.RestoreSystemsAfterPause();
    }
    public void QuitToMenu()
    {
        PauseInput.ClearSuppressedSystems();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveGame()
    {
        SaveManager.SaveCurrentGame();
    }

    private void OnDestroy()
    {
        PauseInput.ClearSuppressedSystems();
    }
} 
