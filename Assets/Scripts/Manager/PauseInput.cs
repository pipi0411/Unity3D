using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseInput : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu PauseScene chưa mở → mở pause
            if (!IsPauseSceneLoaded())
            {
                Time.timeScale = 0f;  // Tạm dừng game
                SceneManager.LoadSceneAsync("PauseScene", LoadSceneMode.Additive);
            }
        }
    }

    private bool IsPauseSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "PauseScene")
            {
                return true;
            }
        }
        return false;
    }
}