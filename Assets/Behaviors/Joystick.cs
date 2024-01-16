using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class JoystickBehavior
    : GenericEntityBehavior<JoystickBehavior, JoystickComponent>
{
    public enum JoystickAlignment { HORIZONTAL, VERTICAL }

    public static new BehaviorType objectType = new BehaviorType(
        "Joystick", typeof(JoystickBehavior))
    {
        displayName = s => s.JoystickName,
        description = s => s.JoystickDesc,
        iconName = "joystick",
        rule = BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public float speed = 1;
    public Target facing = new Target(Target.NO_DIRECTION);
    public JoystickAlignment alignment = JoystickAlignment.HORIZONTAL;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", s => s.PropSpeed,
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float),
            new Property("dir", s => s.PropFacing,
                () => facing,
                v => facing = (Target)v,
                PropertyGUIs.TargetFacing),
            new Property("alg", s => s.PropAlignment,
                () => alignment,
                v => alignment = (JoystickAlignment)v,
                PropertyGUIs.Enum),
        });
}

public class JoystickComponent : MotionComponent<JoystickBehavior>
{
    public override Vector3 GetTranslateFixed()
    {
        Vector3 forward;
        if (behavior.facing.direction == Target.NO_DIRECTION && PlayerComponent.instance != null)
            forward = PlayerComponent.instance.transform.forward;
        else
            forward = behavior.facing.DirectionFrom(transform);
        forward = forward.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        float x = CrossPlatformInputManager.GetAxis("Horizontal");
        float y = CrossPlatformInputManager.GetAxis("Vertical");
        Vector3 control = x * right;
        if (behavior.alignment == JoystickBehavior.JoystickAlignment.VERTICAL)
            control += new Vector3(0, y, 0);
        else
            control += y * forward;

        return control * behavior.speed * Time.fixedDeltaTime;
    }
}
