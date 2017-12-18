using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : EntityBehavior
{
    float speed = 50;

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
                PropertyGUIs.Float)
        });
        return props;
    }

    public override Component MakeComponent(GameObject gameObject)
    {
        SpinComponent spin = gameObject.AddComponent<SpinComponent>();
        spin.speed = speed;
        return spin;
    }
}

public class SpinComponent : MonoBehaviour
{
    public float speed = 50;

    void Update()
    {
        transform.Rotate(Vector3.up, speed / 60);
    }
}