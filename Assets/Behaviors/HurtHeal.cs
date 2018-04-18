using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtHealBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Hurt/Heal", "",
        "heart", typeof(HurtHealBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private float amount = -30;
    private float rate = 0;
    private float minHealth = 0;
    private float maxHealth = 9999;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Amount",
                () => amount,
                v => amount = (float)v,
                PropertyGUIs.Float),
            new Property("Rate",
                () => rate,
                v => rate = (float)v,
                PropertyGUIs.Time),
            new Property("Min health",
                () => minHealth,
                v => minHealth = (float)v,
                PropertyGUIs.Float),
            new Property("Max health",
                () => maxHealth,
                v => maxHealth = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        HurtHealComponent component = gameObject.AddComponent<HurtHealComponent>();
        component.amount = amount;
        component.rate = rate;
        component.minHealth = minHealth;
        component.maxHealth = maxHealth;
        return component;
    }
}

public class HurtHealComponent : MonoBehaviour
{
    public float amount, rate, minHealth, maxHealth;
    private float lastTime;
    private bool started = false;

    void Start()
    {
        started = true;
        if (enabled)
            OnEnable();
    }

    void OnEnable()
    {
        if (!started)
            return;
        HurtHeal();
        lastTime = Time.time;
    }

    void Update()
    {
        if (rate != 0 && Time.time - lastTime >= rate)
        {
            HurtHeal();
            lastTime += rate;
        }
    }

    private void HurtHeal()
    {
        DynamicEntityComponent dynamicEntity = GetComponent<DynamicEntityComponent>();
        if (dynamicEntity == null)
            return;
        float health = dynamicEntity.health;
        if (health < minHealth || health > maxHealth)
            return;
        health += amount;
        if (health < minHealth)
            health = minHealth;
        if (health > maxHealth)
            health = maxHealth;
        float diff = health - dynamicEntity.health;
        if (diff > 0)
            dynamicEntity.Heal(diff);
        else if (diff < 0)
            dynamicEntity.Hurt(-diff);
    }
}