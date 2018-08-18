using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    private UnityEngine.UI.Text text;
    private float lastHealth = 100f;
    bool hurt;
    private float healthChangeTime = -10f;

    void Start()
    {
        text = GetComponent<UnityEngine.UI.Text>();
    }

    void Update()
    {
        PlayerComponent player = PlayerComponent.instance;
        if (player == null)
            return;
        text.text = "Health: " + (int)(player.health);
        //text.text = (int)(1.0f / Time.smoothDeltaTime) + " FPS";

        if (player.health != lastHealth)
        {
            healthChangeTime = Time.time;
            hurt = player.health < lastHealth;
            lastHealth = player.health;
        }

        if (Time.time - healthChangeTime < 1.0)
        {
            if (hurt)
                text.color = Color.Lerp(Color.red, Color.black, Time.time - healthChangeTime);
            else
                text.color = Color.Lerp(Color.green, Color.black, Time.time - healthChangeTime);
        }
        else
            text.color = Color.black;
    }
}
