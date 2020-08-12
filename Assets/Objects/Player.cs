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

    public override Vector3 PositionOffset()
    {
        return new Vector3(0, -0.5f, 0);
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
    public static PlayerComponent instance;
    public int score = 0;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }
}