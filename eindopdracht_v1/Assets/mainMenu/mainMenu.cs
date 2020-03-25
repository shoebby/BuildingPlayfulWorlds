using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void PlayLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void PlayTest()
    {
        SceneManager.LoadScene(2);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}