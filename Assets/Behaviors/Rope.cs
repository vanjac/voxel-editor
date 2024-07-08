using System;
using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior]
public class RopeBehavior : GenericEntityBehavior<RopeBehavior, RopeComponent> {
    public static new BehaviorType objectType = new BehaviorType("Rope", typeof(RopeBehavior)) {
        displayName = s => s.RopeName,
        description = s => s.RopeDesc,
        iconName = "lasso",
        rule = BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public EntityReference target = new EntityReference(null);
    public Color color = Color.black;
    public float width = 0.1f;
    public float length = 5f;
    public bool physics = true;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[] {
            new Property("obj", s => s.PropTarget,
                () => target,
                v => target = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("col", s => s.PropColor,
                () => color,
                v => color = (Color)v,
                PropertyGUIs.Color),
            new Property("wid", s => s.PropWidth,
                () => width,
                v => width = (float)v,
                PropertyGUIs.Slider(0.0f, 1.0f)),
            new Property("len", s => s.PropLength,
                () => length,
                v => length = (float)v,
                PropertyGUIs.Float),
            new Property("phy", s => s.PropPhysicsEnable,
                () => physics,
                v => physics = (bool)v,
                PropertyGUIs.Toggle),
        });
}

public class RopeComponent : BehaviorComponent<RopeBehavior> {
    private const int NUM_POINTS = 16;
    private const int MAX_ITERATIONS = 8;
    private const float EPSILON = 1e-6f;

    private LineRenderer lineRenderer;
    private SpringJoint springJoint;

    public override void Start() {
        if (behavior.width > 0) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = Resources.Load<Material>("RopeMaterial");
            lineRenderer.startColor = lineRenderer.endColor = behavior.color;
            lineRenderer.startWidth = lineRenderer.endWidth = behavior.width;
        }

        base.Start();
    }

    public override void BehaviorEnabled() {
        if (behavior.physics && behavior.target.component != null) {
            springJoint = gameObject.AddComponent<SpringJoint>();
            springJoint.connectedBody = behavior.target.component.GetComponent<Rigidbody>();
            springJoint.minDistance = 0;
            springJoint.maxDistance = behavior.length;
            springJoint.enableCollision = true;
        }
    }

    public override void BehaviorDisabled() {
        if (lineRenderer) {
            lineRenderer.enabled = false;
        }
        if (springJoint) {
            Destroy(springJoint);
        }
    }

    void Update() {
        if (!lineRenderer) {
            // nothing
        } else if (behavior.target.component != null) {
            lineRenderer.enabled = true;
            renderRope(transform.position, behavior.target.component.transform.position);
        } else if (behavior.target.entity != null && CompareTag("EditorPreview")) {
            lineRenderer.enabled = true;
            renderRope(transform.position, behavior.target.entity.PositionInEditor());
        } else {
            lineRenderer.enabled = false;
        }
    }

    private void renderRope(Vector3 p1, Vector3 p2) {
        var dist = Vector3.Distance(p1, p2);
        if (dist < EPSILON) {
            lineRenderer.positionCount = 0;
        } else if (dist >= behavior.length) {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, p1);
            lineRenderer.SetPosition(1, p2);
        } else {
            renderCaternaryCurve(p1, p2);
        }
    }

    private void renderCaternaryCurve(Vector3 p1, Vector3 p2) {
        lineRenderer.positionCount = NUM_POINTS;

        // https://www.alanzucconi.com/2020/12/13/catenary-2/
        // https://github.com/dulnan/catenary-curve/
        //      Copyright 2011 poiasd
        //      Copyright 2018 Jan Hug <me@dulnan.net>

        float hLen = Vector2.Distance(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
        float vLen = p2.y - p1.y;
        float ropeLen = behavior.length;
        float a = calcCaternaryParameter(hLen, vLen, ropeLen);
        float p = (hLen - a * Mathf.Log((ropeLen + vLen) / (ropeLen - vLen))) / 2.0f;
        float q = p1.y - a * (float)Math.Cosh(p / a);
        for (int i = 0; i < NUM_POINTS; i++) {
            float t = i / (NUM_POINTS - 1.0f);
            Vector3 pos = Vector3.Lerp(p1, p2, t);
            pos.y = a * (float)Math.Cosh(((t * hLen) - p) / a) + q;
            lineRenderer.SetPosition(i, pos);
        }
    }

    private float calcCaternaryParameter(float hLen, float vLen, float ropeLen) {
        float m = Mathf.Sqrt(ropeLen * ropeLen - vLen * vLen) / hLen;
        float x = Mathf.Log(m + Mathf.Sqrt(m * m - 1)) + 1;
        float prevX = -1;

        for (int i = 0; i < MAX_ITERATIONS && Mathf.Abs(x - prevX) > EPSILON; i++) {
            prevX = x;
            x -= ((float)Math.Sinh(x) - m * x) / ((float)Math.Cosh(x) - m);
        }

        return hLen / (2 * x);
    }
}
