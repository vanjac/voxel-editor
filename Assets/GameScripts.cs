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

    public static string[] ListNames(NamedType[] namedTypes)
    {
        var names = new string[namedTypes.Length];
        for (int i = 0; i < namedTypes.Length; i++)
            names[i] = namedTypes[i].name;
        return names;
    }

    public static NamedType FindWithName(NamedType[] namedTypes, string name)
    {
        for (int i = 0; i < namedTypes.Length; i++)
            if (namedTypes[i].name == name)
                return namedTypes[i];
        return new NamedType("", null);
    }

    public static NamedType[] behaviors = new NamedType[]
    {
        new NamedType("Spin", typeof(Spin))
    };


}