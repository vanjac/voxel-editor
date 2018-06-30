using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePause : MonoBehaviour
{
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