using System.Collections.Generic;
using UnityEngine;

public class CheckScoreSensor : GenericSensor<CheckScoreSensor, CheckScoreComponent> {
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
            "Check Score", typeof(CheckScoreSensor)) {
        displayName = s => s.CheckScoreName,
        description = s => s.CheckScoreDesc,
        iconName = "code-greater-than-or-equal",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public enum AboveOrBelow {
        ABOVE, BELOW
    }

    public int threshold = 100;
    public AboveOrBelow compare = AboveOrBelow.ABOVE;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[] {
            new Property("cmp", s => s.PropScoreIs,
                () => compare,
                v => compare = (AboveOrBelow)v,
                PropertyGUIs.EnumIcons(
                    new Texture[]{ GUIPanel.IconSet.greaterEqual, GUIPanel.IconSet.lessEqual })),
            new Property("thr", s => s.PropThreshold,
                () => threshold,
                v => threshold = (int)v,
                PropertyGUIs.Int)
        }, base.Properties());
}

public class CheckScoreComponent : SensorComponent<CheckScoreSensor> {
    void Update() {
        var player = PlayerComponent.instance;
        if (player == null) {
            RemoveActivator(null);
        } else {
            if (sensor.compare == CheckScoreSensor.AboveOrBelow.ABOVE) {
                if (player.score >= sensor.threshold) {
                    AddActivator(null);
                } else {
                    RemoveActivator(null);
                }
            } else {
                if (player.score <= sensor.threshold) {
                    AddActivator(null);
                } else {
                    RemoveActivator(null);
                }
            }
        }
    }
}
