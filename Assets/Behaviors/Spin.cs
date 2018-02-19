using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Spin", "Rotate continuously", "format-rotate-90", typeof(Spin));

    private float speed = 50;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Speed",
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        SpinComponent spin = gameObject.AddComponent<SpinComponent>();
        spin.speed = speed;
        return spin;
    }
}

public class SpinComponent : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}