using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void Start()
    {
        // Tạm dừng game khi vào pause
        Time.timeScale = 0f;

        // Khôi phục chuột để click được nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;

        // Khôi phục chuột (dự phòng)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load lại SampleScene (giống cách từ MainMenu)
        SceneManager.LoadScene("SampleScene");
    }
         public void QuitToMenu()
     {
        Time.timeScale = 1f;
         SceneManager.LoadScene("MainMenu");
     }
} 
