using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using SimpleJSON;

public class MapFileWriter {
    public const int VERSION = 0;
    private const int FILE_MIN_READER_VERSION = 0;

    private string fileName;

    public MapFileWriter(string fileName)
    {
        this.fileName = fileName;
    }

    public void Write(Transform cameraPivot, VoxelArray voxelArray)
    {
        JSONObject root = new JSONObject();

        root["writerVersion"] = VERSION;
        root["minReaderVersion"] = FILE_MIN_READER_VERSION;

        JSONObject camera = new JSONObject();
        camera["pan"] = WriteVector3(cameraPivot.position);
        camera["rotate"] = WriteQuaternion(cameraPivot.rotation);
        camera["scale"] = cameraPivot.localScale.z;
        root["camera"] = camera;

        root["world"] = WriteWorld(voxelArray);

        string filePath = Application.persistentDataPath + "/" + fileName + ".json.gz";
        using (FileStream fileStream = File.Create(filePath))
        {
            using (GZipStream zipStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                using (var sw = new StreamWriter(zipStream))
                {
                    sw.Write(root.ToString());
                    sw.Flush();
                }
            }
        }
    }

    private JSONObject WriteWorld(VoxelArray voxelArray)
    {
        JSONObject world = new JSONObject();

        JSONArray materialsArray = new JSONArray();
        var foundMaterials = new List<string>();
        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            foreach (VoxelFace face in voxel.faces)
            {
                Material material = face.material;
                if (material == null)
                    continue;
                string name = material.name;
                if (!foundMaterials.Contains(name))
                {
                    foundMaterials.Add(name);
                    JSONObject materialObject = new JSONObject();
                    materialObject["name"] = name;
                    materialsArray[-1] = materialObject;
                }
            }
        }
        world["materials"] = materialsArray;

        world["map"] = WriteMap(voxelArray, foundMaterials);

        return world;
    }

    private JSONObject WriteMap(VoxelArray voxelArray, List<string> materials)
    {
        JSONObject map = new JSONObject();
        JSONArray voxels = new JSONArray();
        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            if (voxel.IsEmpty())
            {
                Debug.Log("Empty voxel found!");
                voxelArray.VoxelModified(voxel);
                continue;
            }
            voxels[-1] = WriteVoxel(voxel, materials);
        }
        map["voxels"] = voxels;
        return map;
    }

    private JSONObject WriteVoxel(Voxel voxel, List<string> materials)
    {
        JSONObject voxelObject = new JSONObject();
        voxelObject["at"] = WriteIntVector3(voxel.transform.position);
        JSONArray faces = new JSONArray();
        for (int faceI = 0; faceI < 6; faceI++)
        {
            VoxelFace face = voxel.faces[faceI];
            if (face.IsEmpty())
                continue;
            faces[-1] = WriteFace(face, faceI, materials);
        }
        voxelObject["f"] = faces;

        return voxelObject;
    }

    private JSONObject WriteFace(VoxelFace face, int faceI, List<string> materials)
    {
        JSONObject faceObject = new JSONObject();
        faceObject["i"].AsInt = faceI;
        if (face.material != null)
        {
            faceObject["mat"].AsInt = materials.IndexOf(face.material.name);
        }
        return faceObject;
    }

    private JSONArray WriteVector3(Vector3 v)
    {
        JSONArray a = new JSONArray();
        a[0] = v.x;
        a[1] = v.y;
        a[2] = v.z;
        return a;
    }

    private JSONArray WriteQuaternion(Quaternion v)
    {
        return WriteVector3(v.eulerAngles);
    }

    private JSONArray WriteIntVector3(Vector3 v)
    {
        JSONArray a = new JSONArray();
        a[0] = Mathf.RoundToInt(v.x);
        a[1] = Mathf.RoundToInt(v.y);
        a[2] = Mathf.RoundToInt(v.z);
        return a;
    }
}
