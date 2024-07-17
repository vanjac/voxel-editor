using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : ObjectEntity {
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
            "Player", typeof(PlayerObject)) {
        displayName = s => s.PlayerName,
        description = s => s.PlayerDesc,
        iconName = "human-greeting",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public bool footstepSounds = true;

    public PlayerObject() {
        // default value when loading from older worlds
        paint.material = AssetPack.InstantiateMaterial(AssetPack.FindMaterial("GLOSSY", true));
        paint.material.color = Color.green;
    }

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[] {
            new Property("wlk", s => s.PropFootstepSounds,
                () => footstepSounds,
                v => footstepSounds = (bool)v,
                PropertyGUIs.Toggle),
        });

    protected override Vector3 PositionInGame() => base.PositionInGame() + new Vector3(0, 1, 0);

    protected override ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray) {
        GameObject markerObject = Resources.Load<GameObject>("ObjectMarkers/Player");
        markerObject = GameObject.Instantiate(markerObject);
        return markerObject.AddComponent<ObjectMarker>();
    }

    protected override DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray) {
        GameObject playerObject = Resources.Load<GameObject>("ObjectPrefabs/Player");
        playerObject = GameObject.Instantiate(playerObject);
        var character = new CharacterBehavior();
        character.density = 3;
        var characterComponent = (CharacterComponent)character.MakeComponent(playerObject);
        characterComponent.volume = 2;
        characterComponent.calculateVolumeAndMass = false;
        var playerComponent = playerObject.AddComponent<PlayerComponent>();
        playerComponent.obj = this;
        return playerComponent;
    }
}

public class PlayerComponent : DynamicEntityComponent {
    public static PlayerComponent instance;
    public PlayerObject obj;
    public int score = 0;
    public bool hasScore = false;

    void Awake() {
        instance = this;
    }

    void OnDestroy() {
        instance = null;
    }
}