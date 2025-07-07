using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playBtn()
    {
        SceneManager.LoadScene("Juego");
    }
    public void tutorialBtn()
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void exitBtn()
    {
        Application.Quit();
    }
}
