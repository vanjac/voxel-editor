using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Player", "The character you control in the game", "human-greeting", typeof(PlayerObject));

    private VoxelArray voxelArray;

    public PlayerObject(VoxelArray array)
    {
        voxelArray = array;
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override void InitObjectMarker()
    {
        GameObject markerObject = Resources.Load<GameObject>("ObjectMarkers/Player");
        markerObject = GameObject.Instantiate(markerObject);
        markerObject.transform.parent = voxelArray.transform;
        marker = markerObject.AddComponent<ObjectMarker>();
        marker.objectEntity = this;
    }

    public override void InitEntityGameObject()
    {
        GameObject playerObject = Resources.Load<GameObject>("ObjectPrefabs/Player");
        playerObject = GameObject.Instantiate(playerObject);
        playerObject.transform.parent = voxelArray.transform;
        playerObject.transform.position = position + new Vector3(0.5f, 0.0f, 0.5f);
        PlayerComponent component = playerObject.AddComponent<PlayerComponent>();
        component.entity = this;
        component.health = health;
        this.component = component;
    }
}

public class PlayerComponent : DynamicEntityComponent
{

}