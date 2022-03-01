using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;
using MsgPack;

public class JSONWorldReader : WorldFileReader
{
    private const int MAX_INPUT_VERSION = 7;
    private const int MIN_INPUT_VERSION = 6; // v1.1.5-beta or later

    private JSONNode rootNode;
    private List<string> warnings = new List<string>();
    // maps the combined mat+over JSON indices to separate msgpack indices
    private Dictionary<int, int> baseIndices;
    private Dictionary<int, int> overlayIndices = new Dictionary<int, int>();
    int maxBaseIndex, maxOverlayIndex;

    public void ReadStream(Stream stream)
    {
        string jsonString;
        try
        {
            using (var sr = new StreamReader(stream))
            {
                jsonString = sr.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            throw new MapReadException("An error occurred while reading the file", e);
        }
        try
        {
            rootNode = JSON.Parse(jsonString);
        }
        catch (Exception e)
        {
            throw new MapReadException("Invalid world file", e);
        }
        if (rootNode == null)
            throw new MapReadException("Invalid world file");
    }

    public List<EmbeddedData> FindEmbeddedData(EmbeddedDataType type)
    {
        return new List<EmbeddedData>();
    }

    public List<string> GetCustomMaterialCategories(PaintLayer layer)
    {
        return new List<string>();
    }

    public List<CustomMaterial> FindCustomMaterials(PaintLayer layer, string category)
    {
        return new List<CustomMaterial>();
    }

    public List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        MessagePackObjectDictionary world;
        try
        {
            world = ConvertRoot(rootNode.AsObject);
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("Error converting world file", e);
        }

        var messagePackReader = new MessagePackWorldReader();
        messagePackReader.UseMessagePackObject(new MessagePackObject(world));
        warnings.AddRange(messagePackReader.BuildWorld(cameraPivot, voxelArray, editor));
        return warnings;
    }

    public MessagePackObjectDictionary ConvertRoot(JSONObject root)
    {
        baseIndices = new Dictionary<int, int>();
        overlayIndices = new Dictionary<int, int>();
        maxBaseIndex = 0;
        maxOverlayIndex = 0;

        if (root == null || root["writerVersion"] == null || root["minReaderVersion"] == null)
        {
            throw new MapReadException("Invalid world file");
        }
        int minReaderVersion = root["minReaderVersion"].AsInt;
        if (minReaderVersion < MIN_INPUT_VERSION)
        {
            throw new MapReadException("This file was created with an old (beta) version of the app and can no longer be read.");
        }
        else if (minReaderVersion > MAX_INPUT_VERSION)
        {
            throw new MapReadException("Invalid world file");
        }

        var worldDict = new MessagePackObjectDictionary();
        // this converter writes in version 8 format
        // however it copies the fileWriterVersion and minReaderVersion from the original JSON
        // to clearly mark that this is a converted file in case this ever needs to be checked
        worldDict[FileKeys.WORLD_WRITER_VERSION] = root["writerVersion"].AsInt;
        worldDict[FileKeys.WORLD_MIN_READER_VERSION] = minReaderVersion;

        if (root["camera"] != null)
        {
            worldDict[FileKeys.WORLD_CAMERA] = new MessagePackObject(
                ConvertCamera(root["camera"].AsObject));
        }
        if (root["world"] != null)
            ConvertWorld(root["world"].AsObject, worldDict);
        
        return worldDict;
    }

    private MessagePackObjectDictionary ConvertCamera(JSONObject cameraObject)
    {
        var cameraDict = new MessagePackObjectDictionary();
        if (cameraObject["pan"] != null)
            cameraDict[FileKeys.CAMERA_PAN] = ConvertFloatArray(cameraObject["pan"].AsArray);
        if (cameraObject["rotate"] != null)
            cameraDict[FileKeys.CAMERA_ROTATE] = ConvertFloatArray(cameraObject["rotate"].AsArray);
        if (cameraObject["scale"] != null)
            cameraDict[FileKeys.CAMERA_SCALE] = cameraObject["scale"].AsFloat;
        return cameraDict;
    }

    // flattened into existing world dictionary
    private void ConvertWorld(JSONObject worldObject, MessagePackObjectDictionary worldDict)
    {
        // save materials for last!

        if (worldObject["substances"] != null)
        {
            var substancesList = new List<MessagePackObject>();
            foreach (JSONNode subNode in worldObject["substances"].AsArray)
                substancesList.Add(new MessagePackObject(ConvertEntity(subNode.AsObject, false)));
            worldDict[FileKeys.WORLD_SUBSTANCES] = new MessagePackObject(substancesList);
        }

        if (worldObject["global"] != null)
        {
            worldDict[FileKeys.WORLD_GLOBAL] = new MessagePackObject(
                ConvertPropertiesObject(worldObject["global"].AsObject, false));
        }

        if (worldObject["map"] != null && worldObject["map"]["voxels"] != null)
        {
            var voxelsList = new List<MessagePackObject>();
            foreach (JSONNode voxelNode in worldObject["map"]["voxels"].AsArray)
                voxelsList.Add(ConvertVoxel(voxelNode.AsObject));
            worldDict[FileKeys.WORLD_VOXELS] = new MessagePackObject(voxelsList);
        }

        if (worldObject["objects"] != null)
        {
            var objectsList = new List<MessagePackObject>();
            foreach (JSONNode objNode in worldObject["objects"].AsArray)
                objectsList.Add(new MessagePackObject(ConvertObjectEntity(objNode.AsObject)));
            worldDict[FileKeys.WORLD_OBJECTS] = new MessagePackObject(objectsList);
        }

        if (worldObject["materials"] != null)
        {
            var bases = new MessagePackObject[maxBaseIndex];
            var overlays = new MessagePackObject[maxOverlayIndex];
            int i = 0;
            foreach (JSONNode matNode in worldObject["materials"].AsArray)
            {
                MessagePackObject matObj = new MessagePackObject(
                    ConvertMaterial(matNode.AsObject, false));
                if (baseIndices.TryGetValue(i, out int baseIndex))
                    bases[baseIndex] = matObj;
                if (overlayIndices.TryGetValue(i, out int overlayIndex))
                    overlays[overlayIndex] = matObj;
                i++;
            }
            worldDict[FileKeys.WORLD_BASE_MATERIALS] = new MessagePackObject(bases);
            worldDict[FileKeys.WORLD_OVERLAY_MATERIALS] = new MessagePackObject(overlays);
        }
    }

    private MessagePackObjectDictionary ConvertMaterial(JSONObject matObject, bool specifyAlphaMode)
    {
        var materialDict = new MessagePackObjectDictionary();
        if (matObject["name"] != null)
            materialDict[FileKeys.MATERIAL_NAME] = matObject["name"].Value;
        if (matObject["mode"] != null)
            materialDict[FileKeys.MATERIAL_MODE] = matObject["mode"].Value;
        if (matObject["color"] != null)
            materialDict[FileKeys.MATERIAL_COLOR] = ConvertFloatArray(matObject["color"].AsArray);
        if (specifyAlphaMode && matObject["alpha"] != null)
            materialDict[FileKeys.MATERIAL_ALPHA] = matObject["color"].AsBool;
        return materialDict;
    }

    private MessagePackObjectDictionary ConvertObjectEntity(JSONObject entityObject)
    {
        var entityDict = ConvertEntity(entityObject, true);
        if (entityObject["at"] != null)
            entityDict[FileKeys.OBJECT_POSITION] = ConvertIntArray(entityObject["at"].AsArray);
        if (entityObject["rotate"] != null)
            entityDict[FileKeys.OBJECT_ROTATION] = entityObject["rotate"].AsFloat;
        return entityDict;
    }

    private MessagePackObjectDictionary ConvertEntity(JSONObject entityObject, bool includeName)
    {
        var entityDict = ConvertPropertiesObject(entityObject, includeName);

        if (entityObject["sensor"] != null)
        {
            entityDict[FileKeys.ENTITY_SENSOR] = new MessagePackObject(
                ConvertPropertiesObject(entityObject["sensor"].AsObject, true));
        }

        if (entityObject["behaviors"] != null)
        {
            var behaviorsList = new List<MessagePackObject>();
            foreach (JSONNode behaviorNode in entityObject["behaviors"].AsArray)
            {
                behaviorsList.Add(new MessagePackObject(
                    ConvertPropertiesObject(behaviorNode.AsObject, true)));
            }
            entityDict[FileKeys.ENTITY_BEHAVIORS] = new MessagePackObject(behaviorsList);
        }

        if (entityObject["id"] != null)
        {
            entityDict[FileKeys.ENTITY_ID] = entityObject["id"].Value;
        }
        
        return entityDict;
    }

    private MessagePackObjectDictionary ConvertPropertiesObject(JSONObject propsObject, bool includeName)
    {
        var propsDict = new MessagePackObjectDictionary();
        if (includeName && propsObject["name"] != null)
            propsDict[FileKeys.PROPOBJ_NAME] = propsObject["name"].Value;

        if (propsObject["properties"] != null)
        {
            var propertiesList = new List<MessagePackObject>();
            foreach (JSONNode propNode in propsObject["properties"].AsArray)
            {
                JSONArray propArray = propNode.AsArray;
                string name = propArray[0];
                string id;
                if (!propNamesToIDs.TryGetValue(name, out id))
                {
                    warnings.Add("Unrecognized property: " + name);
                    continue;
                }

                var propList = new List<MessagePackObject>();
                propList.Add(id);
                if (name == "Material" || name == "Sky")
                    propList.Add(new MessagePackObject(ConvertMaterial(propArray[1].AsObject, true)));
                else
                    propList.Add(propArray[1].Value);

                if (propArray.Count > 2)
                    propList.Add(propArray[2].Value);

                propertiesList.Add(new MessagePackObject(propList));
            }
            propsDict[FileKeys.PROPOBJ_PROPERTIES] = new MessagePackObject(propertiesList);
        }

        return propsDict;
    }

    private MessagePackObject ConvertVoxel(JSONObject voxelObject)
    {
        var voxelList = new List<MessagePackObject>();
        if (voxelObject["at"] == null)
            return new MessagePackObject(voxelList);
        voxelList.Add(ConvertIntArray(voxelObject["at"].AsArray));

        var facesList = new List<MessagePackObject>();
        if (voxelObject["f"] != null)
        {
            foreach (JSONNode faceNode in voxelObject["f"].AsArray)
                facesList.Add(ConvertFace(faceNode.AsObject));
        }
        voxelList.Add(new MessagePackObject(facesList));

        bool hasEdges = voxelObject["e"] != null;

        if (voxelObject["s"] != null)
            voxelList.Add(voxelObject["s"].AsInt);
        else if (hasEdges) // need to pad this index
            voxelList.Add(-1);

        if (hasEdges)
        {
            var edgesList = new List<MessagePackObject>();
            foreach (JSONNode edgeNode in voxelObject["e"].AsArray)
                edgesList.Add(ConvertEdge(edgeNode.AsObject));
            voxelList.Add(new MessagePackObject(edgesList));
        }

        return new MessagePackObject(voxelList);
    }

    private MessagePackObject ConvertFace(JSONObject faceObject)
    {
        var faceList = new List<MessagePackObject>();
        if (faceObject["i"] == null)
            return new MessagePackObject(faceList);
        faceList.Add(faceObject["i"].AsInt);
        if (faceObject["mat"] != null)
        {
            int mat = faceObject["mat"].AsInt;
            if (!baseIndices.TryGetValue(mat, out int index))
                baseIndices[mat] = index = maxBaseIndex++;
            faceList.Add(index);
        }
        else
        {
            faceList.Add(-1);
        }
        if (faceObject["over"] != null)
        {
            int over = faceObject["over"].AsInt;
            if (!overlayIndices.TryGetValue(over, out int index))
                overlayIndices[over] = index = maxOverlayIndex++;
            faceList.Add(index);
        }
        else
        {
            faceList.Add(-1);
        }
        if (faceObject["orient"] != null)
            faceList.Add(faceObject["orient"].AsInt);
        while (faceList[faceList.Count - 1].AsInt32() == -1)
            faceList.RemoveAt(faceList.Count - 1);
        return new MessagePackObject(faceList);
    }

    private MessagePackObject ConvertEdge(JSONObject edgeObject)
    {
        var edgeList = new List<MessagePackObject>();
        if (edgeObject["i"] == null)
            return new MessagePackObject(edgeList);
        edgeList.Add(edgeObject["i"].AsInt);
        if (edgeObject["bevel"] != null)
            edgeList.Add(edgeObject["bevel"].AsInt);
        return new MessagePackObject(edgeList);
    }

    private MessagePackObject ConvertFloatArray(JSONArray a)
    {
        var l = new List<MessagePackObject>();
        foreach (JSONNode val in a)
            l.Add(val.AsFloat);
        return new MessagePackObject(l);
    }

    private MessagePackObject ConvertIntArray(JSONArray a)
    {
        var l = new List<MessagePackObject>();
        foreach (JSONNode val in a)
            l.Add(val.AsInt);
        return new MessagePackObject(l);
    }

    // all properties from v1.2.2, mapped to IDs from v1.2.3
    private static Dictionary<string, string> propNamesToIDs = new Dictionary<string, string>
    {
        // Entity
        {"Tag",                 "tag"},
        // Dynamic Entity
        {"X-Ray?",              "xra"},
        {"Health",              "hea"},
        // World
        {"Sky",                 "sky"},
        {"Ambient light intensity", "amb"},
        {"Sun intensity",       "sin"},
        {"Sun color",           "sco"},
        {"Sun pitch",           "spi"},
        {"Sun yaw",             "sya"},
        {"Fog density",         "fdn"},
        {"Fog color",           "fco"},
        // Ball
        {"Material",            "mat"},

        /* BEHAVIORS */

        // Common
        {"Target",              "tar"},
        {"Condition",           "con"},
        // Clone (extends Teleport)
        // Force
        {"Mode",                "fmo"},
        {"Ignore mass?",        "ima"},
        {"Stop object first?",  "sto"},
        {"Strength",            "mag"},
        {"Toward",              "dir"},
        // Hurt/Heal
        {"Amount",              "num"},
        {"Rate",                "rat"},
        {"Min health",          "min"},
        {"Max health",          "max"},
        // Light
        {"Size",                "siz"},
        {"Color",               "col"},
        {"Intensity",           "int"},
        {"Halo?",               "hal"},
        // Move
        {"Speed",               "vel"},
        // {"Toward",           "dir"}, // already in Force
        // Move With
        {"Parent",              "par"},
        {"Follow rotation?",    "fro"},
        // Physics
        {"Density",             "den"},
        {"Gravity?",            "gra"},
        // Solid (no properties)
        // Spin
        // {"Speed",            "vel"}, // already in Move
        // Teleport
        {"To",                  "loc"},
        {"Relative to",         "rel"},
        // Visible (no properties)
        // Water
        // {"Density",          "den"}, // already in Physics

        /* SENSORS */

        // ActivatedSensor
        {"Filter",              "fil"},
        // Delay
        {"Input",               "inp"},
        {"Off time",            "oft"},
        {"On time",             "ont"},
        {"Start on?",           "sta"},
        // In Camera
        {"Max distance",        "dis"},
        // In Range
        {"Distance",            "dis"},
        // Threshold
        {"Threshold",           "thr"},
        {"Inputs",              "inp"},
        // Motion
        {"Min velocity",        "vel"},
        {"Min angular vel.",    "ang"},
        {"Direction",           "dir"},
        // Pulse
        // {"Start on?",        "sta"}, // already in Delay
        // {"Off time",         "oft"}, // already in Delay
        // {"On time",          "ont"}, // already in Delay
        // Tap
        // {"Max distance",     "dis"}, // already in In Camera
        // Toggle
        // {"Start on?",        "sta"}, // already in Delay
        {"Off input",           "ofi"},
        {"On input",            "oni"},
        // Touch
        // {"Min velocity",     "vel"}, // already in Motion
        // {"Direction",        "dir"}, // already in Motion
    };
}
