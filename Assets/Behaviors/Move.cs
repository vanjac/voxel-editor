using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Move", "Move in a direction or toward an object",
        "arrow-right-bold-box-outline", typeof(MoveBehavior));

    private Target target = new Target(0);
    private float speed = 1;

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
                PropertyGUIs.Float),
            new Property("Target",
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Empty)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        MoveComponent move = gameObject.AddComponent<MoveComponent>();
        move.target = target;
        move.speed = speed;
        return move;
    }
}

public class MoveComponent : MonoBehaviour
{
    public Target target;
    public float speed;

    void Update()
    {
        Vector3 move = target.directionFrom(transform.position);
        move *= speed * Time.deltaTime;
        transform.Translate(move);
    }
}
