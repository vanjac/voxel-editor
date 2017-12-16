using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : EntityBehavior
{
    float speed;

    public override string TypeName()
    {
        return "Spin";
    }

    public override ICollection<Property> Properties()
    {
        var props = new List<Property>(base.Properties());
        props.AddRange(new Property[]
        {
            new Property("Speed",
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Empty)
        });
        return props;
    }
}