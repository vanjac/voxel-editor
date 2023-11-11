using System.Collections.Generic;
using UnityEngine;

public class HurtHealBehavior : GenericEntityBehavior<HurtHealBehavior, HurtHealComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Hurt/Heal", "Lose/gain health; below 0, object dies",
        "•  <b>Amount</b>: Change in health. Positive heals, negative hurts.\n"
        + "•  <b>Rate</b>: Seconds between successive hurt/heals. 0 means health will only change once when behavior is activated.\n"
        + "•  <b>Keep within</b>: Health will only change if it's within this range, and will never go outside this range.",
        "heart", typeof(HurtHealBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public float amount = -30;
    public float rate = 0;
    public (float, float) healthRange = (0, 200);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("num", "Amount",
                () => amount,
                v => amount = (float)v,
                PropertyGUIs.Float),
            new Property("rat", "Rate",
                () => rate,
                v => rate = (float)v,
                PropertyGUIs.Time),
            new Property("ran", "Keep within",
                () => healthRange,
                v => healthRange = ((float, float))v,
                PropertyGUIs.FloatRange)
        });

    public override IEnumerable<Property> DeprecatedProperties() =>
        Property.JoinProperties(base.DeprecatedProperties(), new Property[]
        {
            new Property("min", "Min health",
                () => healthRange.Item1,
                v => healthRange.Item1 = (float)v,
                PropertyGUIs.Float),
            new Property("max", "Max health",
                () => healthRange.Item2,
                v => healthRange.Item2 = (float)v,
                PropertyGUIs.Float)
        });
}

public class HurtHealComponent : BehaviorComponent<HurtHealBehavior>
{
    private float lastTime;

    public override void BehaviorEnabled()
    {
        HurtHeal();
        lastTime = Time.time;
    }

    void Update()
    {
        if (behavior.rate != 0 && Time.time - lastTime >= behavior.rate)
        {
            HurtHeal();
            lastTime += behavior.rate;
        }
    }

    private void HurtHeal()
    {
        DynamicEntityComponent dynamicEntity = GetComponent<DynamicEntityComponent>();
        if (dynamicEntity == null)
            return;
        float health = dynamicEntity.health;
        if (health < behavior.healthRange.Item1 || health > behavior.healthRange.Item2)
            return;
        health += behavior.amount;
        if (health < behavior.healthRange.Item1)
            health = behavior.healthRange.Item1;
        if (health > behavior.healthRange.Item2)
            health = behavior.healthRange.Item2;
        float diff = health - dynamicEntity.health;
        if (diff > 0)
            dynamicEntity.Heal(diff);
        else if (diff < 0)
            dynamicEntity.Hurt(-diff);
    }
}