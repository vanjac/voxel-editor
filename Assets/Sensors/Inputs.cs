using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputsSensor : Sensor
{
    // public so it can be serialized
    public struct Input
    {
        Entity entity;
        byte onChange;
        byte offChange;
    }

    public int threshold;
    private Input[] inputs = new Input[0];

    public override string TypeName()
    {
        return "Inputs";
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Threshold",
                () => threshold,
                v => threshold = (int)v,
                PropertyGUIs.Int),
            new Property("Inputs",
                () => inputs,
                v => inputs = (Input[])v,
                PropertyGUIs.Empty)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }
}