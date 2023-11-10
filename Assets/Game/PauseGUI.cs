using UnityEngine;

public class HUDCounter
{
    private readonly string text;
    private int lastValue = -1;
    private float changeTime = -10f;
    private bool negativeChange;

    public HUDCounter(string text)
    {
        this.text = text;
    }

    public void Update(int value)
    {
        if (lastValue != -1 && lastValue != value)
        {
            changeTime = Time.time;
            negativeChange = value < lastValue;
        }
        lastValue = value;
        Display();
    }

    public void Display()
    {
        Color baseColor = GUI.color;
        if (Time.time - changeTime < 1.0)
        {
            if (negativeChange)
                GUI.color *= Color.Lerp(Color.red, Color.white, Time.time - changeTime);
            else
                GUI.color *= Color.Lerp(Color.green, Color.white, Time.time - changeTime);
        }
        ActionBarGUI.ActionBarLabel(text + lastValue);
        GUI.color = baseColor;
    }
}

// based on MenuOverflowGUI
public class PauseGUI : GUIPanel
{
    public GameLoad gameLoad;
    private OverflowMenuGUI pauseMenu;
    private FadeGUI fade;

    private bool paused = false;
    private bool wasAlive = false;

    private HUDCounter healthCounter;
    private HUDCounter scoreCounter;

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
        healthCounter = new HUDCounter("Health: ");
        scoreCounter = new HUDCounter("Score: ");
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
            AudioListener.pause = false;
            Destroy(fade);
        }

        if (pauseMenu != null)
            pauseMenu.BringToFront();

        GUILayout.BeginHorizontal();

        PlayerComponent player = PlayerComponent.instance;
        if (player != null)
        {
            wasAlive = true;

            healthCounter.Update((int)player.health);
            scoreCounter.Update(player.score);
        }
        else if (wasAlive)
        {
            ActionBarGUI.ActionBarLabel("you died :(");
            scoreCounter.Display();
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
        AudioListener.pause = true;
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
                gameLoad.Close(Scenes.GAME);
            }),
            new OverflowMenuGUI.MenuItem("Editor", GUIIconSet.instance.editor, () =>
            {
                gameLoad.Close(Scenes.EDITOR);
            }),
            new OverflowMenuGUI.MenuItem("Close", GUIIconSet.instance.x, () =>
            {
                gameLoad.Close(Scenes.MENU);
            })
        };

        fade = gameObject.AddComponent<FadeGUI>();
    }
}