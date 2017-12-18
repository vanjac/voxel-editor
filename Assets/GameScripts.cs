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

        public object Instantiate()
        {
            return System.Activator.CreateInstance(type);
        }
    }

    public static string[] ListNames(NamedType[] namedTypes, bool includeNone=false)
    {
        int numItems = namedTypes.Length;
        if (includeNone)
            numItems += 1;
        var names = new string[numItems];
        if (includeNone)
        {
            names[0] = "None";
            numItems = 1;
        }
        else
            numItems = 0;
        for (int i = 0; i < namedTypes.Length; i++)
        {
            names[numItems] = namedTypes[i].name;
            numItems++;
        }
        return names;
    }

    public static NamedType FindWithName(NamedType[] namedTypes, string name)
    {
        for (int i = 0; i < namedTypes.Length; i++)
            if (namedTypes[i].name == name)
                return namedTypes[i];
        return new NamedType("", null);
    }


    public static NamedType[] sensors = new NamedType[]
    {
        new NamedType("Pulse", typeof(Pulse))
    };

    public static NamedType[] behaviors = new NamedType[]
    {
        new NamedType("Spin", typeof(Spin))
    };

}