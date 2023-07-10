using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Water", "Simulates buoyancy for player and physics objects",
        "Water should not be Solid and should not have Physics. This behavior controls only the physics of water, not appearance.",
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
            new Property("den", "Density",
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

public class WaterComponent : BehaviorComponent
{
    public float density;
    private float waterLevel = float.MinValue;

    public override void Start()
    {
        SubstanceComponent substanceComponent = GetComponent<SubstanceComponent>();
        if (substanceComponent != null)
        {
            foreach (Voxel voxel in substanceComponent.substance.voxelGroup.IterateVoxels())
            {
                float top = voxel.GetBounds().max.y - transform.position.y;
                if (top > waterLevel)
                    waterLevel = top;
            }
        }
        base.Start();
    }

    public float GetWaterLevel(float x, float z)
    {
        return waterLevel + transform.position.y;
    }
}