using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionComponent : BehaviorComponent
{
    private Rigidbody rigidBody;

    public override void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        base.Start();
    }

    void OnCollisionEnter()
    {
        // TODO: this might not still be necessary
        if (rigidBody != null)
            rigidBody.velocity = Vector3.zero;
    }

    // should include subclasses
    public override void LastBehaviorDisabled()
    {
        if (rigidBody != null)
            rigidBody.constraints = RigidbodyConstraints.None;
    }

    // should be called in FixedUpdate
    // amount should usually be the same value as GetMoveFixed()
    void FixedUpdate()
    {
        if (rigidBody != null)
            rigidBody.velocity = Vector3.zero;
        Vector3 translate = GetTranslateFixed();
        Quaternion rotate = GetRotateFixed();
        if (rigidBody != null)
        {
            var e = GetComponent<DynamicEntityComponent>();
            e.RigidbodyRotate(rigidBody, rotate);
            e.RigidbodyTranslate(rigidBody, translate, true);
        }
        else
        {
            Vector3 axis;
            float angle;
            rotate.ToAngleAxis(out angle, out axis);
            transform.Rotate(axis, angle);
            transform.Translate(translate);
        }
    }

    // player standing on top of object will move this amount
    public virtual Vector3 GetTranslateFixed()
    {
        return Vector3.zero;
    }

    public virtual Quaternion GetRotateFixed()
    {
        return Quaternion.identity;
    }
}