using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;
using System.Xml;
using System.Xml.Serialization;

public class JSONWorldReader : WorldFileReader
{
    public const int VERSION = 7;

    private int fileWriterVersion;

    private JSONNode rootNode;
    private List<string> warnings = new List<string>();
    private bool editor;

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


    public List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        this.editor = editor;

        JSONObject root = rootNode.AsObject;
        if (root == null || root["writerVersion"] == null || root["minReaderVersion"] == null)
        {
            throw new MapReadException("Invalid world file");
        }
        if (root["minReaderVersion"].AsInt > VERSION)
        {
            throw new MapReadException("This world file requires a newer version of the app");
        }
        fileWriterVersion = root["writerVersion"].AsInt;

        EntityReference.ResetEntityIds();

        try
        {
            if (editor && cameraPivot != null && root["camera"] != null)
                ReadCamera(root["camera"].AsObject, cameraPivot);
            if (root["world"] != null)
                ReadWorld(root["world"].AsObject, voxelArray);
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("Error reading world file", e);
        }

        EntityReference.DoneLoadingEntities();
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

    private void ReadWorld(JSONObject world, VoxelArray voxelArray)
    {
        var materials = new List<Material>();
        if (world["materials"] != null)
        {
            foreach (JSONNode matNode in world["materials"].AsArray)
            {
                JSONObject matObject = matNode.AsObject;
                materials.Add(ReadMaterial(matObject));
            }
        }

        var substances = new List<Substance>();
        if (world["substances"] != null)
        {
            foreach (JSONNode subNode in world["substances"].AsArray)
            {
                Substance s = new Substance();
                ReadEntity(subNode.AsObject, s);
                substances.Add(s);
            }
        }

        if (world["global"] != null)
            ReadPropertiesObject(world["global"].AsObject, voxelArray.world);
        if (fileWriterVersion <= 2 && world["sky"] != null)
        {
            Material sky = materials[world["sky"].AsInt];
            if (sky != ReadWorldFile.missingMaterial) // default skybox is null
                voxelArray.world.SetSky(sky);
        }
        if (world["map"] != null)
            ReadMap(world["map"].AsObject, voxelArray, materials, substances);
        if (fileWriterVersion <= 2 && world["player"] != null)
        {
            PlayerObject player = new PlayerObject();
            ReadObjectEntity(world["player"].AsObject, player);
            voxelArray.AddObject(player);
        }
        if (world["objects"] != null)
        {
            foreach (JSONNode objNode in world["objects"].AsArray)
            {
                JSONObject objObject = objNode.AsObject;
                string typeName = objObject["name"];
                var objType = GameScripts.FindTypeWithName(GameScripts.objects, typeName);
                if (objType == null)
                {
                    warnings.Add("Unrecognized object type: " + typeName);
                    continue;
                }
                ObjectEntity obj = (ObjectEntity)objType.Create();
                ReadObjectEntity(objObject, obj);
                voxelArray.AddObject(obj);
            }
        }

        if (!editor)
        {
            // start the game
            foreach (Substance s in substances)
                s.InitEntityGameObject(voxelArray);
            foreach (ObjectEntity obj in voxelArray.IterateObjects())
                obj.InitEntityGameObject(voxelArray);
        }
        else // editor
        {
            foreach (ObjectEntity obj in voxelArray.IterateObjects())
                obj.InitObjectMarker((VoxelArrayEditor)voxelArray);
        }
    }

    private Material ReadMaterial(JSONObject matObject)
    {
        if (matObject["name"] != null)
        {
            string name = matObject["name"];
            Material mat = ResourcesDirectory.FindMaterial(name, editor);
            if (mat == null)
            {
                warnings.Add("Unrecognized material: " + name);
                return ReadWorldFile.missingMaterial;
            }
            return mat;
        }
        else if (matObject["mode"] != null)
        {
            ColorMode mode = (ColorMode)System.Enum.Parse(typeof(ColorMode), matObject["mode"]);
            if (matObject["color"] != null)
            {
                Color color = ReadColor(matObject["color"].AsArray);
                bool alpha = color.a != 1;
                if (matObject["alpha"] != null)
                    alpha = matObject["alpha"].AsBool; // new with version 4
                Material mat = ResourcesDirectory.MakeCustomMaterial(mode, alpha);
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
            warnings.Add("Error reading material");
            return ReadWorldFile.missingMaterial;
        }
    }

    private void ReadObjectEntity(JSONObject entityObject, ObjectEntity objectEntity)
    {
        ReadEntity(entityObject, objectEntity);
        if (entityObject["at"] != null)
            objectEntity.position = ReadVector3Int(entityObject["at"].AsArray);
        if (entityObject["rotate"] != null)
            objectEntity.rotation = entityObject["rotate"].AsFloat;
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
                warnings.Add("Unrecognized sensor: " + sensorName);
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
                    warnings.Add("Unrecognized behavior: " + behaviorName);
                    continue;
                }
                EntityBehavior newBehavior = (EntityBehavior)behaviorType.Create();
                ReadPropertiesObject(behaviorObject, newBehavior);
                if (fileWriterVersion <= 5 && behaviorObject["target"] != null)
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
                Property prop = new Property(null, null, null, null, null);
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
                    warnings.Add("Unrecognized property: " + name);
                    continue;
                }

                System.Type propType;
                if (propArray.Count > 2)
                    propType = System.Type.GetType(propArray[2]); // explicit type
                else
                    propType = prop.value.GetType();

                if (propType == typeof(Material))
                {
                    // skip equality check
                    prop.setter(ReadMaterial(propArray[1].AsObject));
                }
                else
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(propType);
                    using (var textReader = new StringReader(valueString))
                    {
                        // skip equality check. important if this is an EntityReference,
                        // since EntityReference.Equals gets the entity which may not exist yet
                        prop.setter(xmlSerializer.Deserialize(textReader));
                    }
                }
            }
        }
    }

    private void ReadMap(JSONObject map, VoxelArray voxelArray,
        List<Material> materials, List<Substance> substances)
    {
        if (map["voxels"] != null)
        {
            foreach (JSONNode voxelNode in map["voxels"].AsArray)
            {
                JSONObject voxelObject = voxelNode.AsObject;
                ReadVoxel(voxelObject, voxelArray, materials, substances);
            }
        }
    }

    private void ReadVoxel(JSONObject voxelObject, VoxelArray voxelArray,
        List<Material> materials, List<Substance> substances)
    {
        if (voxelObject["at"] == null)
            return;
        Vector3Int position = ReadVector3Int(voxelObject["at"].AsArray);
        Voxel voxel = null;
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
                    voxel = voxelArray.VoxelAt(position + Voxel.DirectionForFaceI(faceI).ToInt(), true);
                }
                ReadFace(faceObject, voxel, materials);
            }
        }

        if (voxelObject["e"] != null)
            foreach (JSONNode edgeNode in voxelObject["e"].AsArray)
                ReadEdge(edgeNode.AsObject, voxel);
        voxel.UpdateVoxel();
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

    private void ReadEdge(JSONObject edgeObject, Voxel voxel)
    {
        if (edgeObject["i"] == null)
            return;
        int edgeI = edgeObject["i"].AsInt;
        if (edgeObject["bevel"] != null)
            voxel.edges[edgeI].bevel = (byte)(edgeObject["bevel"].AsInt);
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
