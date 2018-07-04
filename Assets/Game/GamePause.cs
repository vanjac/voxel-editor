using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePause : MonoBehaviour
{
    public UnityEngine.UI.Text titleText;
    public GameObject resumeButton; // hidden after death
    private bool alive = false;
    private bool dead = false;

    public void PauseGame()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Time.timeScale = 1;
    }

    void Update()
    {
        Camera cam = Camera.current;
        if (!alive)
        {
            if (cam != null && cam.tag == "MainCamera")
                alive = true;
        }
        if (alive && !dead)
        {
            if (cam != null && cam.tag == "DeathCamera")
            {
                PauseGame();
                dead = true;
                titleText.text = "you died :(";
                resumeButton.SetActive(false);
            }
        }

        if (Input.GetButtonDown("Cancel"))
            if (GetComponent<CanvasGroup>().blocksRaycasts)
                ResumeGame();
            else
                PauseGame();
    }

    void OnDestroy()
    {
        Time.timeScale = 1;
    }
}