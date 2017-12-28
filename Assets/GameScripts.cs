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
        Spin.objectType
    };

}