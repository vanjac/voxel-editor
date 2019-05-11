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
    private int slot;

    public override void Start()
    {
        TextAsset songAsset = Resources.Load<TextAsset>("burningrome");
        slot = SunVoxUtils.OpenUnusedSlot();
        int result = SunVox.sv_load_from_memory(slot, songAsset.bytes, songAsset.bytes.Length);
        if (result != 0)
        {
            Debug.LogError("Error loading file");
            return;
        }
        Debug.Log(System.Runtime.InteropServices.Marshal.PtrToStringAuto(SunVox.sv_get_song_name(0)));

        base.Start();
    }

    public void OnDestroy()
    {
        SunVoxUtils.CloseSlot(slot);
    }

    public override void BehaviorEnabled()
    {
        SunVox.sv_play_from_beginning(slot);
    }

    public override void BehaviorDisabled()
    {
        SunVox.sv_stop(slot);
    }
}