using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtHealBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Hurt/Heal", "Lose or gain health; below zero health, object will die",
        "•  <b>Amount</b>: Change in health. Positive heals, negative hurts.\n"
        + "•  <b>Rate</b>: Seconds between successive hurt/heals. 0 means health will only change once when behavior is activated.\n"
        + "•  <b>Keep within</b>: Health will only change if it's within this range, and will never go outside this range.",
        "heart", typeof(HurtHealBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    [FloatProp("num", "Amount")]
    public float amount { get; set; } = -30;
    [TimeProp("rat", "Rate")]
    public float rate { get; set; } = 0;
    [FloatRangeProp("ran", "Keep within")]
    public (float, float) healthRange { get; set; } = (0, 200);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override IEnumerable<Property> DeprecatedProperties()
    {
        return Property.JoinProperties(base.DeprecatedProperties(), new Property[]
        {
            new Property("min", "Min health",
                () => healthRange.Item1,
                v => healthRange = ((float)v, healthRange.Item2),
                PropertyGUIs.Float),
            new Property("max", "Max health",
                () => healthRange.Item2,
                v => healthRange = (healthRange.Item1, (float)v),
                PropertyGUIs.Float)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        HurtHealComponent component = gameObject.AddComponent<HurtHealComponent>();
        component.amount = amount;
        component.rate = rate;
        component.minHealth = healthRange.Item1;
        component.maxHealth = healthRange.Item2;
        return component;
    }
}

public class HurtHealComponent : BehaviorComponent
{
    public float amount, rate, minHealth, maxHealth;
    private float lastTime;

    public override void BehaviorEnabled()
    {
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