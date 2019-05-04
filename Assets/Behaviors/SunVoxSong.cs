using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunVoxSongBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Song", "Play a song created with SunVox",
        "sunvox", typeof(SunVoxSongBehavior));

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {

        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<SunVoxSongComponent>();
    }
}


public class SunVoxSongComponent : BehaviorComponent
{
    public override void Start()
    {
        TextAsset songAsset = Resources.Load<TextAsset>("burningrome");

        int version = SunVox.sv_init("0", 44100, 2, 0);
        if (version < 0)
        {
            Debug.LogError("Error initializing SunVox");
            return;
        }

        int major = (version >> 16) & 255;
        int minor1 = (version >> 8) & 255;
        int minor2 = (version) & 255;
        Debug.Log(System.String.Format("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));

        SunVox.sv_open_slot(0);
        int result = SunVox.sv_load_from_memory(0, songAsset.bytes, songAsset.bytes.Length);
        if (result != 0)
        {
            Debug.LogError("Error loading file");
            return;
        }
        Debug.Log(System.Runtime.InteropServices.Marshal.PtrToStringAuto(SunVox.sv_get_song_name(0)));

        base.Start();
    }

    public override void BehaviorEnabled()
    {
        SunVox.sv_play_from_beginning(0);
    }

    public override void BehaviorDisabled()
    {
        SunVox.sv_stop(0);
    }
}