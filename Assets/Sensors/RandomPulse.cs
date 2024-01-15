using System.Collections.Generic;
using UnityEngine;

public class RandomPulseSensor : GenericSensor<RandomPulseSensor, RandomPulseComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Rand. Pulse", typeof(RandomPulseSensor))
    {
        description = s => s.RandomPulseDesc,
        longDescription = s => s.RandomPulseLongDesc,
        iconName = "progress-question",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public (float, float) offTimeRange = (1, 5);
    public (float, float) onTimeRange = (1, 5);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("oft", s => s.PropOffTime,
                () => offTimeRange,
                v => offTimeRange = ((float, float))v,
                PropertyGUIs.FloatRange),
            new Property("ont", s => s.PropOnTime,
                () => onTimeRange,
                v => onTimeRange = ((float, float))v,
                PropertyGUIs.FloatRange)
        }, base.Properties());
}

public class RandomPulseComponent : SensorComponent<RandomPulseSensor>
{
    private const float MIN_PULSE = 1.0f / 30.0f;

    private bool state;
    private float flipTime;

    void Start()
    {
        state = Random.Range(0, 2) == 0;
        if (state)
            AddActivator(null);
        // start at random point in cycle
        flipTime = Time.time + Random.Range(0.0f, WaitTime());
    }

    private float WaitTime()
    {
        if (state)
            return RandomTime(sensor.onTimeRange);
        else
            return RandomTime(sensor.offTimeRange);
    }

    private float RandomTime((float, float) range)
    {
        float min = range.Item1;
        float max = range.Item2;
        if (min < MIN_PULSE)
            min = MIN_PULSE;
        if (max < MIN_PULSE)
            max = MIN_PULSE;
        return Mathf.Exp(Random.Range(Mathf.Log(min), Mathf.Log(max)));
    }

    public void Update()
    {
        if (Time.time >= flipTime)
        {
            state = !state;
            if (state)
                AddActivator(null);
            else
                RemoveActivator(null);
            flipTime += WaitTime();
        }
    }
}