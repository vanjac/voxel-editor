using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SunVoxWorldReader : WorldFileReader
{
    byte[] bytes;

    public void ReadStream(Stream stream)
    {
        bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
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
                PropertiesObjectType.SetProperty(behavior, "dat", new EmbeddedData("imported", bytes));
                obj.behaviors.Add(behavior);
            }
        }
        return warnings;
    }
}