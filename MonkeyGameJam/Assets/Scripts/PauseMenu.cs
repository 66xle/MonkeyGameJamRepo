using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] AudioSource music;
    [SerializeField] Slider sliderMusic;

    private bool isMusicEnabled = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 1)
        {
            menu.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
        }

        Music();
    }
    public void Resume()
    {
        menu.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void Music()
    {
        music.volume = sliderMusic.value;
    }
}
