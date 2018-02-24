using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Player", "The character you control in the game", "human-greeting", typeof(PlayerObject));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }
}

public class PlayerMarker : ObjectMarker
{
    void Awake()
    {
        objectEntity = new PlayerObject();
    }
}