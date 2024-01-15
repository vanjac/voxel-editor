using System.Collections.Generic;

public class WaterBehavior : GenericEntityBehavior<WaterBehavior, WaterComponent>
{
    public static new BehaviorType objectType = new BehaviorType("Water", typeof(WaterBehavior))
    {
        description = s => s.WaterDesc,
        longDescription = s => s.WaterLongDesc,
        iconName = "water",
        rule = BehaviorType.BaseTypeRule(typeof(Substance)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public float density = 1.0f;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("den", s => s.PropDensity,
                () => density,
                v => density = (float)v,
                PropertyGUIs.Float)
        });
}

public class WaterComponent : BehaviorComponent<WaterBehavior>
{
    public float Density => behavior.density;
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

    public float GetWaterLevel(float x, float z) => waterLevel + transform.position.y;
}