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

    protected override ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray)
    {
        GameObject markerObject = Resources.Load<GameObject>("ObjectMarkers/Player");
        markerObject = GameObject.Instantiate(markerObject);
        return markerObject.AddComponent<ObjectMarker>();
    }

    protected override DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray)
    {
        GameObject playerObject = Resources.Load<GameObject>("ObjectPrefabs/Player");
        playerObject = GameObject.Instantiate(playerObject);
        return playerObject.AddComponent<PlayerComponent>();
    }
}

public class PlayerComponent : DynamicEntityComponent
{

}