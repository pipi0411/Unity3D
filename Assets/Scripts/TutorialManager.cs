using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Button backButton; 

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}