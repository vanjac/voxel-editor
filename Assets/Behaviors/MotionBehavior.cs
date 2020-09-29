using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionComponent : BehaviorComponent
{
    private Rigidbody rigidBody;
    private DynamicEntityComponent entityComponent;

    public override void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        entityComponent = gameObject.GetComponent<DynamicEntityComponent>();
        base.Start();
    }

    void OnCollisionEnter()
    {
        // TODO: this might not still be necessary
        if (rigidBody != null && !entityComponent.isCharacter)
            rigidBody.velocity = Vector3.zero;
    }

    // should include subclasses
    public override void LastBehaviorDisabled()
    {
        if (rigidBody != null && !entityComponent.isCharacter)
            rigidBody.constraints = RigidbodyConstraints.None;
    }

    // should be called in FixedUpdate
    // amount should usually be the same value as GetMoveFixed()
    void FixedUpdate()
    {
        if (rigidBody != null && !entityComponent.isCharacter)
            rigidBody.velocity = Vector3.zero;
        Vector3 translate = GetTranslateFixed();
        Quaternion rotate = GetRotateFixed();
        if (entityComponent.isCharacter)
            translate.y = 0;
        if (rigidBody != null)
        {
            entityComponent.RigidbodyRotate(rigidBody, rotate);
            entityComponent.RigidbodyTranslate(rigidBody, translate, !entityComponent.isCharacter);
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