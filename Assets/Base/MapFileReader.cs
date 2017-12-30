using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using SimpleJSON;
using System.Xml;
using System.Xml.Serialization;

public class MapFileReader
{
    public const int VERSION = MapFileWriter.VERSION;

    private string fileName;
    private int fileWriterVersion;

    public MapFileReader(string fileName)
    {
        this.fileName = fileName;
    }

    public void Read(Transform cameraPivot, VoxelArray voxelArray, bool editor)
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
        fileWriterVersion = root["writerVersion"].AsInt;

        if (editor && cameraPivot != null)
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
            ReadWorld(root["world"].AsObject, voxelArray, editor);
    }

    private void ReadWorld(JSONObject world, VoxelArray voxelArray, bool editor)
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
                    string checkFileName = Path.GetFileNameWithoutExtension(newDirEntry);
                    if((!editor) && checkFileName.StartsWith("$")) // special alternate materials for game
                        checkFileName = checkFileName.Substring(1);
                    if (checkFileName == name)
                    {
                        mat = ResourcesDirectory.GetMaterial(newDirEntry);
                        break;
                    }
                }
                if (mat == null)
                    Debug.Log("No material found: " + name);
                materials.Add(mat);
            }
        }

        var substances = new List<Substance>();
        if (world["substances"] != null)
        {
            foreach (JSONNode subNode in world["substances"].AsArray)
            {
                Substance s = new Substance(voxelArray);
                ReadEntity(subNode.AsObject, s);
                substances.Add(s);
            }
        }

        if (world["lighting"] != null)
            ReadLighting(world["lighting"].AsObject, materials);
        if (world["map"] != null)
            ReadMap(world["map"].AsObject, voxelArray, materials, substances, editor);
    }

    private void ReadEntity(JSONObject entityObject, Entity entity)
    {
        ReadPropertiesObject(entityObject, entity);

        if (entityObject["sensor"] != null)
        {
            JSONObject sensorObject = entityObject["sensor"].AsObject;
            string sensorName = sensorObject["name"];
            var sensorType = GameScripts.FindTypeWithName(GameScripts.sensors, sensorName);
            if (sensorType == null)
                Debug.Log("Couldn't find sensor type " + sensorName + "!");
            else
            {
                Sensor newSensor = (Sensor)sensorType.Create();
                ReadPropertiesObject(sensorObject, newSensor);
                entity.sensor = newSensor;
            }
        }

        if (entityObject["behaviors"] != null)
        {
            foreach (JSONNode behaviorNode in entityObject["behaviors"].AsArray)
            {
                JSONObject behaviorObject = behaviorNode.AsObject;
                string behaviorName = behaviorObject["name"];
                var behaviorType = GameScripts.FindTypeWithName(GameScripts.behaviors, behaviorName);
                if (behaviorType == null)
                {
                    Debug.Log("Couldn't find behavior type " + behaviorName + "!");
                    continue;
                }
                EntityBehavior newBehavior = (EntityBehavior)behaviorType.Create();
                ReadPropertiesObject(behaviorObject, newBehavior);
                entity.behaviors.Add(newBehavior);
            }
        }

        if (entityObject["id"] != null)
        {
            System.Guid id = new System.Guid(entityObject["id"]);
            EntityReference.AddExistingEntityId(entity, id);
        }
    }

    private void ReadPropertiesObject(JSONObject propsObject, PropertiesObject obj)
    {
        if (propsObject["properties"] != null)
        {
            foreach (JSONNode propNode in propsObject["properties"].AsArray)
            {
                JSONArray propArray = propNode.AsArray;
                string name = propArray[0];
                string valueString = propArray[1];

                bool foundProp = false;
                Property prop = new Property(null, null, null, null);
                foreach (Property checkProp in obj.Properties())
                {
                    if (checkProp.name == name)
                    {
                        prop = checkProp;
                        foundProp = true;
                        break;
                    }
                }
                if (!foundProp)
                {
                    Debug.Log("Couldn't find property " + name);
                    continue;
                }

                XmlSerializer xmlSerializer = new XmlSerializer(prop.value.GetType());
                object value;
                using (var textReader = new StringReader(valueString))
                {
                    value = xmlSerializer.Deserialize(textReader);
                }

                prop.value = value;
            }
        }
    }

    private void ReadLighting(JSONObject lighting, List<Material> materials)
    {
        if (lighting["sky"] != null)
        {
            Material skybox = materials[lighting["sky"].AsInt];
            if (skybox != null) // default skybox is null
            {
                RenderSettings.skybox = skybox;
            }
        }
        if (lighting["ambientIntensity"] != null)
            RenderSettings.ambientIntensity = lighting["ambientIntensity"].AsFloat;
        if (lighting["sun"] != null)
        {
            JSONObject sun = lighting["sun"].AsObject;
            if (sun["intensity"] != null)
                RenderSettings.sun.intensity = sun["intensity"].AsFloat;
            if (sun["color"] != null)
                RenderSettings.sun.color = ReadColor(sun["color"].AsArray);
            if (sun["angle"] != null)
                RenderSettings.sun.transform.rotation = ReadQuaternion(sun["angle"].AsArray);
        }
    }

    private void ReadMap(JSONObject map, VoxelArray voxelArray,
        List<Material> materials, List<Substance> substances, bool editor)
    {
        if (map["voxels"] != null)
        {
            foreach (JSONNode voxelNode in map["voxels"].AsArray)
            {
                JSONObject voxelObject = voxelNode.AsObject;
                ReadVoxel(voxelObject, voxelArray, materials, substances, editor);
            }
        }
    }

    private void ReadVoxel(JSONObject voxelObject, VoxelArray voxelArray,
        List<Material> materials, List<Substance> substances, bool editor)
    {
        if (voxelObject["at"] == null)
            return;
        Vector3 position = ReadVector3(voxelObject["at"].AsArray);
        Voxel voxel = null;
        if (!editor)
            // slightly faster -- doesn't add to octree
            voxel = voxelArray.InstantiateVoxel(position);
        else
            voxel = voxelArray.VoxelAt(position, true);

        if (voxelObject["s"] != null)
            voxel.substance = substances[voxelObject["s"].AsInt];

        if (voxelObject["f"] != null)
        {
            foreach (JSONNode faceNode in voxelObject["f"].AsArray)
            {
                JSONObject faceObject = faceNode.AsObject;
                if (fileWriterVersion == 0)
                {
                    // faces were oriented differently. Get voxel for each face
                    int faceI = faceObject["i"].AsInt;
                    voxel = voxelArray.VoxelAt(position + Voxel.DirectionForFaceI(faceI), true);
                }
                ReadFace(faceObject, voxel, materials);
            }
        }
    }

    private void ReadFace(JSONObject faceObject, Voxel voxel, List<Material> materials)
    {
        if (faceObject["i"] == null)
            return;
        int faceI = faceObject["i"].AsInt;
        if (fileWriterVersion == 0)
            faceI = Voxel.OppositeFaceI(faceI);
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
        if (faceObject["orient"] != null)
        {
            voxel.faces[faceI].orientation = (byte)(faceObject["orient"].AsInt);
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

    private Color ReadColor(JSONArray a)
    {
        if (a.Count == 4)
            return new Color(a[0], a[1], a[2], a[3]);
        else
            return new Color(a[0], a[1], a[2]);
    }
}
