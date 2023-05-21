using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Play()
    {
        SceneManager.LoadScene(1);
    }

    void Quit()
    {
        Application.Quit();
    }
}
