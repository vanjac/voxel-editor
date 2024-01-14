using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Player", s => s.PlayerDesc, "human-greeting", typeof(PlayerObject));
    public override PropertiesObjectType ObjectType => objectType;

    private bool footstepSounds = true;

    public PlayerObject()
    {
        paint.material = ResourcesDirectory.InstantiateMaterial(
            ResourcesDirectory.FindMaterial("GLOSSY", true));
        paint.material.color = Color.green;
    }

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("wlk", "Footstep sounds?",
                () => footstepSounds,
                v => footstepSounds = (bool)v,
                PropertyGUIs.Toggle),
        });

    public override Vector3 PositionOffset() => new Vector3(0, -0.5f, 0);

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
        var character = new CharacterBehavior();
        character.density = 3;
        var characterComponent = (CharacterComponent)character.MakeComponent(playerObject);
        characterComponent.volume = 2;
        characterComponent.calculateVolumeAndMass = false;
        var playerComponent = playerObject.AddComponent<PlayerComponent>();
        playerComponent.footstepSounds = footstepSounds;
        return playerComponent;
    }
}

public class PlayerComponent : DynamicEntityComponent
{
    public static PlayerComponent instance;
    public bool footstepSounds;
    public int score = 0;
    public bool hasScore = false;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }
}