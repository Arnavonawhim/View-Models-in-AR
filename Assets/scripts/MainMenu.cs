using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // This function loads the AR scene by build index
    public void PlayGame()
    {
        // Loads the next scene (index 1 if MainMenu is index 0)
        SceneManager.LoadScene(1);
    }

    // This function quits the game
    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}