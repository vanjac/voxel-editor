using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScripts
{
    public struct NamedType
    {
        public string name;
        public System.Type type;

        public NamedType(string name, System.Type type)
        {
            this.name = name;
            this.type = type;
        }
    }

    public static System.Type FindTypeWithName(NamedType[] namedTypes, string name)
    {
        for (int i = 0; i < namedTypes.Length; i++)
            if (namedTypes[i].name == name)
                return namedTypes[i].type;
        return null;
    }


    public static NamedType[] sensors = new NamedType[]
    {
        new NamedType("None", null),
        new NamedType("Input Threshold", typeof(InputThresholdSensor)),
        new NamedType("Pulse", typeof(PulseSensor)),
        new NamedType("Touch", typeof(TouchSensor))
    };

    public static NamedType[] behaviors = new NamedType[]
    {
        new NamedType("Visible", typeof(Visible)),
        new NamedType("Solid", typeof(Solid)),
        new NamedType("Spin", typeof(Spin))
    };

}