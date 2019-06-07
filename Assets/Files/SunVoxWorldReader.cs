using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SunVoxWorldReader : WorldFileReader
{
    private EmbeddedData data;

    public void ReadStream(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        string name = null;
        int slot = SunVoxUtils.OpenUnusedSlot();
        int result = SunVox.sv_load_from_memory(slot, bytes, bytes.Length);
        if (result == 0)
            name = System.Runtime.InteropServices.Marshal.PtrToStringAuto(SunVox.sv_get_song_name(slot));
        SunVoxUtils.CloseSlot(slot);
        if (name != null)
            name = name.Trim();
        if (name == null || name == "")
            name = "imported";

        data = new EmbeddedData(name, bytes, EmbeddedDataType.SunVox);
    }

    public List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        var warnings = ReadWorldFile.Read(Resources.Load<TextAsset>("default"),
            cameraPivot, voxelArray, editor);
        foreach (var obj in voxelArray.IterateObjects())
        {
            if (obj is PlayerObject)
            {
                var behavior = new SunVoxSongBehavior();
                PropertiesObjectType.SetProperty(behavior, "dat", data);
                obj.behaviors.Add(behavior);
            }
        }
        return warnings;
    }

    public List<EmbeddedData> FindEmbeddedData(EmbeddedDataType type)
    {
        List<EmbeddedData> dataList = new List<EmbeddedData>();
        if (type == EmbeddedDataType.SunVox)
            dataList.Add(data);
        return dataList;
    }
}