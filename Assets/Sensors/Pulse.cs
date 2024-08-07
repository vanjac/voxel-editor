﻿using System.Collections.Generic;
using UnityEngine;

public class PulseSensor : GenericSensor<PulseSensor, PulseComponent> {
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
            "Pulse", typeof(PulseSensor)) {
        displayName = s => s.PulseName,
        description = s => s.PulseDesc,
        longDescription = s => s.PulseLongDesc,
        iconName = "pulse",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public bool startOn = true;
    public float offTime = 1;
    public float onTime = 1;
    public EntityReference input = new EntityReference(null);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[] {
            new Property("sta", s => s.PropStartOn,
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle),
            new Property("oft", s => s.PropOffTime,
                () => offTime,
                v => offTime = (float)v,
                PropertyGUIs.Time),
            new Property("ont", s => s.PropOnTime,
                () => onTime,
                v => onTime = (float)v,
                PropertyGUIs.Time),
            new Property("inp", s => s.PropInput,
                () => input,
                v => input = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull)
        }, base.Properties());
}

public class PulseComponent : SensorComponent<PulseSensor> {
    private float startTime;
    private bool useInput;
    private bool cyclePaused;

    void Start() {
        startTime = Time.time;
        useInput = sensor.input.component != null;
        cyclePaused = useInput;
    }

    public void Update() {
        bool inputIsOn = false;
        if (sensor.input.component != null) {
            inputIsOn = sensor.input.component.IsOn();
        }

        float timePassed = Time.time - startTime;
        if (cyclePaused && inputIsOn) {
            cyclePaused = false;
            startTime = Time.time;
            timePassed = 0;
        } else if (useInput && timePassed >= sensor.offTime + sensor.onTime) {
            if (inputIsOn) {
                while (timePassed >= sensor.offTime + sensor.onTime) {
                    startTime += sensor.offTime + sensor.onTime;
                    timePassed -= sensor.offTime + sensor.onTime;
                }
            } else {
                cyclePaused = true;
            }
        }

        if (cyclePaused) {
            RemoveActivator(null);
        } else {
            bool state;
            float cycleTime = timePassed % (sensor.offTime + sensor.onTime);
            if (sensor.startOn) {
                state = cycleTime < sensor.onTime;
            } else {
                state = cycleTime >= sensor.offTime;
            }
            if (state) {
                AddActivator(null);
            } else {
                RemoveActivator(null);
            }
        }
    }
}