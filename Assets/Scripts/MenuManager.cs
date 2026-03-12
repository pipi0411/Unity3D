using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button endButton;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners(); // Xóa sự kiện cũ nếu có
            startButton.onClick.AddListener(LoadGameScene);
        }

        if (endButton != null)
        {
            endButton.onClick.RemoveAllListeners();
            endButton.onClick.AddListener(QuitGame);
        }
        if (tutorButton != null)
        tutorButton.onClick.AddListener(LoadTutorialScene);
    }

    public void LoadGameScene()  
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()  
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

[SerializeField] private Button tutorButton;  
public void LoadTutorialScene()
{
    SceneManager.LoadScene("TutorialScene");   // Tên scene hướng dẫn
}

}