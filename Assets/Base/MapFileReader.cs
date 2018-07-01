using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using SimpleJSON;
using System.Xml;
using System.Xml.Serialization;

public class MapReadException : Exception
{
    public MapReadException() { }
    public MapReadException(string message) : base(message) { }
    public MapReadException(string message, Exception inner) : base(message, inner) { }
}

public class MapFileReader
{
    public const int VERSION = MapFileWriter.VERSION;

    private string fileName;
    private int fileWriterVersion;
    private Material missingMaterial; // material to be used when material can't be created

    private List<string> warnings = new List<string>();

    public MapFileReader(string fileName)
    {
        this.fileName = fileName;
    }

    // return warnings
    public List<string> Read(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        if (missingMaterial == null)
        {
            // allowTransparency is true in case the material is used for an overlay, so the alpha value can be adjusted
            missingMaterial = ResourcesDirectory.MakeCustomMaterial(ColorMode.UNLIT, true);
            missingMaterial.color = Color.magenta;
        }
        string jsonString;

        try
        {
            string filePath = WorldFiles.GetFilePath(fileName);
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
            {
                using (var sr = new StreamReader(fileStream))
                {
                    jsonString = sr.ReadToEnd();
                }
            }
        }
        catch (Exception e)
        {
            throw new MapReadException("An error occurred while reading the file", e);
        }

        JSONNode rootNode;
        try
        {
            rootNode = JSON.Parse(jsonString);
        }
        catch (Exception e)
        {
            throw new MapReadException("Invalid map file", e);
        }
        if (rootNode == null)
            throw new MapReadException("Invalid map file");
        JSONObject root = rootNode.AsObject;
        if (root == null || root["writerVersion"] == null || root["minReaderVersion"] == null)
        {
            throw new MapReadException("Invalid map file");
        }
        if (root["minReaderVersion"].AsInt > VERSION)
        {
            throw new MapReadException("This map requires a newer version of the app");
        }
        fileWriterVersion = root["writerVersion"].AsInt;

        EntityReference.ResetEntityIds();

        try
        {
            if (editor && cameraPivot != null && root["camera"] != null)
                ReadCamera(root["camera"].AsObject, cameraPivot);
            if (root["world"] != null)
                ReadWorld(root["world"].AsObject, voxelArray, editor);
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("Error reading map file", e);
        }

        return warnings;
    }

    private void ReadCamera(JSONObject camera, Transform cameraPivot)
    {
        if (camera["pan"] != null)
            cameraPivot.position = ReadVector3(camera["pan"].AsArray);
        if (camera["rotate"] != null)
            cameraPivot.rotation = ReadQuaternion(camera["rotate"].AsArray);
        if (camera["scale"] != null)
        {
            float scale = camera["scale"].AsFloat;
            cameraPivot.localScale = new Vector3(scale, scale, scale);
        }
    }

    private void ReadWorld(JSONObject world, VoxelArray voxelArray, bool editor)
    {
        var materials = new List<Material>();
        if (world["materials"] != null)
        {
            foreach (JSONNode matNode in world["materials"].AsArray)
            {
                JSONObject matObject = matNode.AsObject;
                materials.Add(ReadMaterial(matObject, editor));
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

        if (world["global"] != null)
            ReadPropertiesObject(world["global"].AsObject, voxelArray.world);
        if (world["sky"] != null)
        {
            Material sky = materials[world["sky"].AsInt];
            if (sky != missingMaterial) // default skybox is null
                voxelArray.world.SetSky(sky);
        }
        if (world["map"] != null)
            ReadMap(world["map"].AsObject, voxelArray, materials, substances, editor);
        voxelArray.playerObject = new PlayerObject(voxelArray);
        if (world["player"] != null)
            ReadObjectEntity(world["player"].AsObject, voxelArray.playerObject);

        if (!editor)
        {
            // start the game
            foreach (Substance s in substances)
                s.InitEntityGameObject();
            voxelArray.playerObject.InitEntityGameObject();
        }
        if (editor)
            voxelArray.playerObject.InitObjectMarker();
    }

    private Material ReadMaterial(JSONObject matObject, bool editor)
    {
        if (matObject["name"] != null)
        {
            string name = matObject["name"];
            foreach (string dirEntry in ResourcesDirectory.dirList)
            {
                if (dirEntry.Length <= 2)
                    continue;
                string newDirEntry = dirEntry.Substring(2);
                string checkFileName = Path.GetFileNameWithoutExtension(newDirEntry);
                if ((!editor) && checkFileName.StartsWith("$")) // special alternate materials for game
                    checkFileName = checkFileName.Substring(1);
                if (checkFileName == name)
                    return ResourcesDirectory.GetMaterial(newDirEntry);
            }
            warnings.Add("Material not found: " + name);
            return missingMaterial;
        }
        else if (matObject["mode"] != null)
        {
            ColorMode mode = (ColorMode)System.Enum.Parse(typeof(ColorMode), matObject["mode"]);
            if (matObject["color"] != null)
            {
                Color color = ReadColor(matObject["color"].AsArray);
                Material mat = ResourcesDirectory.MakeCustomMaterial(mode, color.a != 1);
                mat.color = color;
                return mat;
            }
            else
            {
                return ResourcesDirectory.MakeCustomMaterial(mode);
            }
        }
        else
        {
            warnings.Add("Couldn't read material");
            return missingMaterial;
        }
    }

    private void ReadObjectEntity(JSONObject entityObject, ObjectEntity objectEntity)
    {
        ReadEntity(entityObject, objectEntity);
        if (entityObject["at"] != null)
            objectEntity.position = ReadVector3Int(entityObject["at"].AsArray);
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
                warnings.Add("Couldn't find sensor type: " + sensorName);
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
                    warnings.Add("Couldn't find behavior type: " + behaviorName);
                    continue;
                }
                EntityBehavior newBehavior = (EntityBehavior)behaviorType.Create();
                ReadPropertiesObject(behaviorObject, newBehavior);
                if (behaviorObject["target"] != null)
                {
                    if (behaviorObject["target"] == "activator")
                        newBehavior.targetEntityIsActivator = true;
                    else
                        newBehavior.targetEntity = new EntityReference(new System.Guid(behaviorObject["target"]));
                }
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
                    warnings.Add("Couldn't find property: " + name);
                    continue;
                }

                System.Type propType;
                if(propArray.Count > 2)
                    propType = System.Type.GetType(propArray[2]); // explicit type
                else
                    propType = prop.value.GetType();

                XmlSerializer xmlSerializer = new XmlSerializer(propType);
                using (var textReader = new StringReader(valueString))
                {
                    prop.value = xmlSerializer.Deserialize(textReader);
                }
            }
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

    private Vector3Int ReadVector3Int(JSONArray a)
    {
        return new Vector3Int(a[0], a[1], a[2]);
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
