using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    [SerializeField] GameObject winMenu;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        winMenu.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
    }
}
