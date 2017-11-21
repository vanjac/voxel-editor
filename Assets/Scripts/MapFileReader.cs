using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using SimpleJSON;

public class MapFileReader {
    public const int VERSION = MapFileWriter.VERSION;

    private string fileName;

    public MapFileReader(string fileName)
    {
        this.fileName = fileName;
    }

    // from https://stackoverflow.com/a/5730893
    public static void CopyTo(Stream input, Stream output)
    {
        byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
        int bytesRead;

        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }
    }

    public void Read(Transform cameraPivot, VoxelArray voxelArray)
    {
        string jsonString;

        string filePath = Application.persistentDataPath + "/" + fileName + ".json";
        using (FileStream fileStream = File.Open(filePath, FileMode.Open))
        {
            using (var sr = new StreamReader(fileStream))
            {
                jsonString = sr.ReadToEnd();
            }
        }

        JSONNode rootNode = JSON.Parse(jsonString);
        JSONObject root = rootNode.AsObject;
        if (root == null || root["writerVersion"] == null || root["minReaderVersion"] == null)
        {
            Debug.Log("Invalid map file!");
            return;
        }
        if (root["minReaderVersion"].AsInt > VERSION)
        {
            Debug.Log("This map file is for a new version of the editor!");
            return;
        }

        if (cameraPivot != null)
        {
            if (root["camera"]["pan"] != null)
                cameraPivot.position = ReadVector3(root["camera"]["pan"].AsArray);
            if (root["camera"]["rotate"] != null)
                cameraPivot.rotation = ReadQuaternion(root["camera"]["rotate"].AsArray);
            if (root["camera"]["scale"] != null)
            {
                float scale = root["camera"]["scale"].AsFloat;
                cameraPivot.localScale = new Vector3(scale, scale, scale);
            }
        }

        if (root["world"] != null)
            ReadWorld(root["world"].AsObject, voxelArray);
    }

    private void ReadWorld(JSONObject world, VoxelArray voxelArray)
    {
        var materials = new List<Material>();
        if (world["materials"] != null)
        {
            foreach (JSONNode matNode in world["materials"].AsArray)
            {
                JSONObject matObject = matNode.AsObject;
                if (matObject["name"] == null)
                {
                    materials.Add(null);
                    continue;
                }
                string name = matObject["name"];
                Material mat = null;
                foreach (string dirEntry in ResourcesDirectory.dirList)
                {
                    if (dirEntry.Length <= 2)
                        continue;
                    string newDirEntry = dirEntry.Substring(2);
                    if (Path.GetFileNameWithoutExtension(newDirEntry) == name)
                    {
                        string path = Path.GetDirectoryName(newDirEntry) + "/" + Path.GetFileNameWithoutExtension(newDirEntry);
                        mat = Resources.Load<Material>(path);
                        break;
                    }
                }
                if (mat == null)
                    Debug.Log("No material found: " + name);
                materials.Add(mat);
            }
        }

        if (world["map"] != null)
            ReadMap(world["map"].AsObject, voxelArray, materials);
    }

    private void ReadMap(JSONObject map, VoxelArray voxelArray, List<Material> materials)
    {
        voxelArray.ClearAll();
        if (map["voxels"] != null)
        {
            foreach (JSONNode voxelNode in map["voxels"].AsArray)
            {
                JSONObject voxelObject = voxelNode.AsObject;
                ReadVoxel(voxelObject, voxelArray, materials);
            }
        }
        voxelArray.UpdateAll();
    }

    private void ReadVoxel(JSONObject voxelObject, VoxelArray voxelArray, List<Material> materials)
    {
        if (voxelObject["at"] == null)
            return;
        Vector3 position = ReadVector3(voxelObject["at"].AsArray);
        Voxel voxel = voxelArray.VoxelAt(position, true);

        if (voxelObject["f"] != null)
        {
            foreach (JSONNode faceNode in voxelObject["f"].AsArray)
            {
                JSONObject faceObject = faceNode.AsObject;
                ReadFace(faceObject, voxel, materials);
            }
        }
        voxelArray.VoxelModified(voxel);
    }

    private void ReadFace(JSONObject faceObject, Voxel voxel, List<Material> materials)
    {
        if (faceObject["i"] == null)
            return;
        int faceI = faceObject["i"].AsInt;
        if (faceObject["mat"] != null)
        {
            int matI = faceObject["mat"].AsInt;
            voxel.faces[faceI].material = materials[matI];
        }
        if (faceObject["over"] != null)
        {
            int matI = faceObject["over"].AsInt;
            voxel.faces[faceI].overlay = materials[matI];
        }
    }

    private Vector3 ReadVector3(JSONArray a)
    {
        return new Vector3(a[0], a[1], a[2]);
    }

    private Quaternion ReadQuaternion(JSONArray a)
    {
        return Quaternion.Euler(ReadVector3(a));
    }
}
