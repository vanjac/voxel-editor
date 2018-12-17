using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on MenuOverflowGUI
public class PauseGUI : GUIPanel
{
    public GameLoad gameLoad;
    private OverflowMenuGUI pauseMenu;
    private FadeGUI fade;

    private bool paused = false;
    private bool wasAlive = false;

    private float lastHealth = 100f;
    bool hurt; // hurt animation if true, heal if false
    private float healthChangeTime = -10f;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(safeRect.xMin, safeRect.yMin, safeRect.width, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    void Start()
    {
        GUIPanel.topPanel = this;
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }
    public override void WindowGUI()
    {
        if (paused && pauseMenu == null)
        {
            paused = false;
            Time.timeScale = 1;
            Destroy(fade);
        }

        if (pauseMenu != null)
            pauseMenu.BringToFront();

        GUILayout.BeginHorizontal();

        PlayerComponent player = PlayerComponent.instance;
        if (player != null)
        {
            wasAlive = true;
            if (player.health != lastHealth)
            {
                healthChangeTime = Time.time;
                hurt = player.health < lastHealth;
                lastHealth = player.health;
            }

            Color baseColor = GUI.color;
            if (Time.time - healthChangeTime < 1.0)
            {
                if (hurt)
                    GUI.color *= Color.Lerp(Color.red, Color.white, Time.time - healthChangeTime);
                else
                    GUI.color *= Color.Lerp(Color.green, Color.white, Time.time - healthChangeTime);
            }
            ActionBarGUI.ActionBarLabel("Health: " + (int)(player.health));
            GUI.color = baseColor;
        }
        else if (wasAlive)
        {
            ActionBarGUI.ActionBarLabel("you died :(");
        }

        //ActionBarGUI.ActionBarLabel((int)(1.0f / Time.smoothDeltaTime) + " FPS");

        GUILayout.FlexibleSpace();
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.pause))
            PauseGame();
        GUILayout.EndHorizontal();
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        paused = true;

        pauseMenu = gameObject.AddComponent<OverflowMenuGUI>();
        pauseMenu.items = new OverflowMenuGUI.MenuItem[]
        {
            new OverflowMenuGUI.MenuItem("Resume", GUIIconSet.instance.play, () =>
            {
                ; // menu will close
            }),
            new OverflowMenuGUI.MenuItem("Restart", GUIIconSet.instance.restart, () =>
            {
                gameLoad.Close("playScene");
            }),
            new OverflowMenuGUI.MenuItem("Editor", GUIIconSet.instance.editor, () =>
            {
                gameLoad.Close("editScene");
            }),
            new OverflowMenuGUI.MenuItem("Close", GUIIconSet.instance.x, () =>
            {
                gameLoad.Close("menuScene");
            })
        };

        fade = gameObject.AddComponent<FadeGUI>();
    }
}