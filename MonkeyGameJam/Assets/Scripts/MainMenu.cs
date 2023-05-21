using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject creditsMenu;
    private bool isCreditsDisplayed = false;

    public void Play()
    {
        SceneManager.LoadScene(1);
        Cursor.visible = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void CreditsMenu()
    {
        if (isCreditsDisplayed)
        {
            isCreditsDisplayed = false;
            creditsMenu.SetActive(false);
        }
        else
        {
            isCreditsDisplayed = true;
            creditsMenu.SetActive(true);
        }
    }

}
