using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScripts
{
    public static PropertiesObjectType FindTypeWithName(PropertiesObjectType[] types, string name)
    {
        for (int i = 0; i < types.Length; i++)
            if (types[i].fullName == name)
                return types[i];
        return null;
    }


    public static PropertiesObjectType[] entityTemplates = new PropertiesObjectType[]
    {
        new PropertiesObjectType("Solid Substance",
            "A block that is solid and opaque by default",
            "cube",
            typeof(Substance),
            () => {
                Substance substance = new Substance(VoxelArrayEditor.instance);
                substance.behaviors.Add(new Visible());
                substance.behaviors.Add(new Solid());
                return substance;
            })
    };

    public static PropertiesObjectType[] sensors = new PropertiesObjectType[]
    {
        PropertiesObjectType.NONE,
        InputThresholdSensor.objectType,
        PulseSensor.objectType,
        TouchSensor.objectType
    };

    public static PropertiesObjectType[] behaviors = new PropertiesObjectType[]
    {
        Visible.objectType,
        Solid.objectType,
        PhysicsBehavior.objectType,
        Move.objectType,
        Spin.objectType
    };

    public static PropertiesObjectType[] entityFilterTypes = new PropertiesObjectType[]
    {
        Entity.objectType,
        Substance.objectType
    };

}