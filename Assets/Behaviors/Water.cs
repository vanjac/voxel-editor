using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Water", "Simulates buoyancy for player and physics objects",
        "water", typeof(WaterBehavior), BehaviorType.BaseTypeRule(typeof(Substance)));

    private float density = 1.0f;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Density",
                () => density,
                v => density = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        WaterComponent water = gameObject.AddComponent<WaterComponent>();
        water.density = density;
        return water;
    }
}

public class WaterComponent : MonoBehaviour
{
    public float density;
    public float waterLevel = float.MinValue;

    void Start()
    {
        SubstanceComponent substanceComponent = GetComponent<SubstanceComponent>();
        if (substanceComponent != null)
        {
            foreach (Voxel voxel in substanceComponent.substance.voxels)
            {
                Bounds bounds = voxel.GetBounds();
                float top = bounds.max.y;
                if (top > waterLevel)
                    waterLevel = top;
            }
        }
    }
}