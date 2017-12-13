using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : EntityBehavior
{
    float speed;

    public override ICollection<Property> DynamicProperties()
    {
        var props = new List<Property>(base.DynamicProperties());
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