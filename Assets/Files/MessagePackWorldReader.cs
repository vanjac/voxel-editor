using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MsgPack;
using System.Xml;
using System.Xml.Serialization;


public class MessagePackWorldReader : WorldFileReader
{
    public const int VERSION = MessagePackWorldWriter.VERSION;

    private MessagePackObject worldObject;
    private List<string> warnings = new List<string>();
    private bool editor;

    public void ReadStream(Stream stream)
    {
        // skip the first identifying byte 'm'
        stream.ReadByte();
        try
        {
            worldObject = Unpacking.UnpackObject(stream);
        }
        catch (UnpackException e)
        {
            throw new InvalidMapFileException(e);
        }
        catch (MessageTypeException e)
        {
            throw new InvalidMapFileException(e);
        }
        if (!worldObject.IsDictionary)
            throw new InvalidMapFileException();
    }


    public List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        this.editor = editor;

        MessagePackObjectDictionary worldDict = worldObject.AsDictionary();
        CheckWorldValid(worldDict);

        EntityReference.ResetEntityIds();

        try
        {
            ReadWorld(worldDict, cameraPivot, voxelArray);
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

    private void CheckWorldValid(MessagePackObjectDictionary worldDict)
    {
        if (!worldDict.ContainsKey(FileKeys.WORLD_WRITER_VERSION)
            || !worldDict.ContainsKey(FileKeys.WORLD_MIN_READER_VERSION))
            throw new InvalidMapFileException();

        if (worldDict[FileKeys.WORLD_MIN_READER_VERSION].AsInt32() > VERSION)
        {
            throw new MapReadException("This world file requires a newer version of the app");
        }
    }

    private void ReadWorld(MessagePackObjectDictionary world, Transform cameraPivot, VoxelArray voxelArray)
    {
        if (world.ContainsKey(FileKeys.WORLD_TYPE))
            voxelArray.type = (VoxelArray.WorldType)world[FileKeys.WORLD_TYPE].AsInt32();

        if (editor && cameraPivot != null && world.ContainsKey(FileKeys.WORLD_CAMERA))
            ReadCamera(world[FileKeys.WORLD_CAMERA].AsDictionary(), cameraPivot);

        var materials = new List<Material>();
        if (world.ContainsKey(FileKeys.WORLD_MATERIALS))
        {
            foreach (var matObj in world[FileKeys.WORLD_MATERIALS].AsList())
                materials.Add(ReadMaterial(matObj.AsDictionary(), false));
        }

        var overlays = new List<Material>();
        if (world.ContainsKey(FileKeys.WORLD_OVERLAYS))
        {
            foreach (var matObj in world[FileKeys.WORLD_OVERLAYS].AsList())
                overlays.Add(ReadMaterial(matObj.AsDictionary(), true));
        }

        var substances = new List<Substance>();
        if (world.ContainsKey(FileKeys.WORLD_SUBSTANCES))
        {
            foreach (var subObj in world[FileKeys.WORLD_SUBSTANCES].AsList())
            {
                Substance s = new Substance();
                ReadEntity(subObj.AsDictionary(), s);
                substances.Add(s);
            }
        }

        if (world.ContainsKey(FileKeys.WORLD_GLOBAL))
            ReadPropertiesObject(world[FileKeys.WORLD_GLOBAL].AsDictionary(), voxelArray.world);

        if (world.ContainsKey(FileKeys.WORLD_VOXELS))
        {
            foreach (var voxelObj in world[FileKeys.WORLD_VOXELS].AsList())
                ReadVoxel(voxelObj, voxelArray, materials, overlays, substances);
        }

        if (world.ContainsKey(FileKeys.WORLD_OBJECTS))
        {
            foreach (var objObj in world[FileKeys.WORLD_OBJECTS].AsList())
            {
                var objDict = objObj.AsDictionary();
                string typeName = objDict[FileKeys.PROPOBJ_NAME].AsString();
                var objType = GameScripts.FindTypeWithName(GameScripts.objects, typeName);
                if (objType == null)
                {
                    warnings.Add("Unrecognized object type: " + typeName);
                    continue;
                }
                ObjectEntity obj = (ObjectEntity)objType.Create();
                ReadObjectEntity(objDict, obj);
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

    private void ReadCamera(MessagePackObjectDictionary camera, Transform cameraPivot)
    {
        if (camera.ContainsKey(FileKeys.CAMERA_PAN))
            cameraPivot.position = ReadVector3(camera[FileKeys.CAMERA_PAN]);
        if (camera.ContainsKey(FileKeys.CAMERA_ROTATE))
            cameraPivot.rotation = ReadQuaternion(camera[FileKeys.CAMERA_ROTATE]);
        if (camera.ContainsKey(FileKeys.CAMERA_SCALE))
        {
            float scale = camera[FileKeys.CAMERA_SCALE].AsSingle();
            cameraPivot.localScale = new Vector3(scale, scale, scale);
        }
    }

    private Material ReadMaterial(MessagePackObjectDictionary matDict, bool overlay)
    {
        string name;

        if (matDict.ContainsKey(FileKeys.MATERIAL_NAME))
        {
            name = matDict[FileKeys.MATERIAL_NAME].AsString();
        }
        else if (matDict.ContainsKey(FileKeys.MATERIAL_MODE))  // version 9 and earlier
        {
            name = matDict[FileKeys.MATERIAL_MODE].AsString();
            if (matDict.ContainsKey(FileKeys.MATERIAL_ALPHA))
                overlay = matDict[FileKeys.MATERIAL_ALPHA].AsBoolean();
            if (overlay)
                name += "_overlay";
        }
        else
        {
            warnings.Add("Error reading material");
            return ReadWorldFile.missingMaterial;
        }

        Material mat = ResourcesDirectory.FindMaterial(name, editor);
        if (mat == null)
        {
            warnings.Add("Unrecognized material: " + name);
            return ReadWorldFile.missingMaterial;
        }
        if (matDict.ContainsKey(FileKeys.MATERIAL_COLOR))
        {
            mat = ResourcesDirectory.InstantiateMaterial(mat);
            mat.color = ReadColor(matDict[FileKeys.MATERIAL_COLOR]);
        }
        return mat;
    }

    private void ReadObjectEntity(MessagePackObjectDictionary entityDict, ObjectEntity objectEntity)
    {
        ReadEntity(entityDict, objectEntity);
        if (entityDict.ContainsKey(FileKeys.OBJECT_POSITION))
            objectEntity.position = ReadVector3Int(entityDict[FileKeys.OBJECT_POSITION]);
        if (entityDict.ContainsKey(FileKeys.OBJECT_ROTATION))
            objectEntity.rotation = entityDict[FileKeys.OBJECT_ROTATION].AsSingle();
    }

    private void ReadEntity(MessagePackObjectDictionary entityDict, Entity entity)
    {
        ReadPropertiesObject(entityDict, entity);

        if (entityDict.ContainsKey(FileKeys.ENTITY_SENSOR))
        {
            var sensorDict = entityDict[FileKeys.ENTITY_SENSOR].AsDictionary();
            string sensorName = sensorDict[FileKeys.PROPOBJ_NAME].AsString();
            var sensorType = GameScripts.FindTypeWithName(GameScripts.sensors, sensorName);
            if (sensorType == null)
                warnings.Add("Unrecognized sensor: " + sensorName);
            else
            {
                Sensor newSensor = (Sensor)sensorType.Create();
                ReadPropertiesObject(sensorDict, newSensor);
                entity.sensor = newSensor;
            }
        }

        if (entityDict.ContainsKey(FileKeys.ENTITY_BEHAVIORS))
        {
            foreach (var behaviorObj in entityDict[FileKeys.ENTITY_BEHAVIORS].AsList())
            {
                var behaviorDict = behaviorObj.AsDictionary();
                string behaviorName = behaviorDict[FileKeys.PROPOBJ_NAME].AsString();
                var behaviorType = GameScripts.FindTypeWithName(GameScripts.behaviors, behaviorName);
                if (behaviorType == null)
                {
                    warnings.Add("Unrecognized behavior: " + behaviorName);
                    continue;
                }
                EntityBehavior newBehavior = (EntityBehavior)behaviorType.Create();
                ReadPropertiesObject(behaviorDict, newBehavior);
                entity.behaviors.Add(newBehavior);
            }
        }

        if (entityDict.ContainsKey(FileKeys.ENTITY_ID))
        {
            System.Guid id = new System.Guid(entityDict[FileKeys.ENTITY_ID].AsString());
            EntityReference.AddExistingEntityId(entity, id);
        }
    }

    private void ReadPropertiesObject(MessagePackObjectDictionary propsDict, PropertiesObject obj)
    {
        if (propsDict.ContainsKey(FileKeys.PROPOBJ_PROPERTIES))
        {
            foreach (var propObj in propsDict[FileKeys.PROPOBJ_PROPERTIES].AsList())
            {
                var propList = propObj.AsList();
                string id = propList[0].AsString();

                bool foundProp = false;
                Property prop = new Property(null, null, null, null, null);
                foreach (Property checkProp in Property.JoinIterateProperties(
                    obj.Properties(), obj.DeprecatedProperties()))
                {
                    if (checkProp.id == id)
                    {
                        prop = checkProp;
                        foundProp = true;
                        break;
                    }
                }
                if (!foundProp)
                {
                    warnings.Add("Unrecognized property: " + id);
                    continue;
                }

                System.Type propType;
                if (propList.Count > 2)
                    propType = System.Type.GetType(propList[2].AsString()); // explicit type
                else
                    propType = prop.value.GetType();

                if (propType == typeof(Material))
                {
                    // skip equality check
                    prop.setter(ReadMaterial(propList[1].AsDictionary(), true));
                }
                else if (propType == typeof(EmbeddedData))
                {
                    var dataList = propList[1].AsList();
                    var name = dataList[0].AsString();
                    var type = (EmbeddedDataType)System.Enum.Parse(typeof(EmbeddedDataType), dataList[1].AsString());
                    var bytes = dataList[2].AsBinary();
                    prop.setter(new EmbeddedData(name, bytes, type));
                }
                else // not a special type
                {
                    string valueString = propList[1].AsString();
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

    private void ReadVoxel(MessagePackObject voxelObj, VoxelArray voxelArray,
        List<Material> materials, List<Material> overlays, List<Substance> substances)
    {
        var voxelList = voxelObj.AsList();
        if (voxelList.Count == 0)
            return;

        Vector3Int position = ReadVector3Int(voxelList[0]);
        Voxel voxel = null;
        voxel = voxelArray.VoxelAt(position, true);

        if (voxelList.Count >= 2)
        {
            foreach (var faceObj in voxelList[1].AsList())
            {
                ReadFace(faceObj, voxel, materials, overlays);
            }
        }

        if (voxelList.Count >= 3 && voxelList[2].AsInt32() != -1)
            voxel.substance = substances[voxelList[2].AsInt32()];

        if (voxelList.Count >= 4)
            foreach (var edgeObj in voxelList[3].AsList())
                ReadEdge(edgeObj, voxel);

        voxel.UpdateVoxel();
    }

    private void ReadFace(MessagePackObject faceObj, Voxel voxel,
        List<Material> materials, List<Material> overlays)
    {
        var faceList = faceObj.AsList();
        if (faceList.Count == 0)
            return;
        int faceI = faceList[0].AsInt32();
        if (faceList.Count >= 2 && faceList[1].AsInt32() != -1)
            voxel.faces[faceI].material = materials[faceList[1].AsInt32()];
        if (faceList.Count >= 3 && faceList[2].AsInt32() != -1)
            voxel.faces[faceI].overlay = overlays[faceList[2].AsInt32()];
        if (faceList.Count >= 4)
            voxel.faces[faceI].orientation = faceList[3].AsByte();
    }

    private void ReadEdge(MessagePackObject edgeObj, Voxel voxel)
    {
        var edgeList = edgeObj.AsList();
        if (edgeList.Count == 0)
            return;
        int edgeI = edgeList[0].AsInt32();
        if (edgeList.Count >= 2)
            voxel.edges[edgeI].bevel = edgeList[1].AsByte();
    }

    private Vector3 ReadVector3(MessagePackObject o)
    {
        var l = o.AsList();
        return new Vector3(l[0].AsSingle(), l[1].AsSingle(), l[2].AsSingle());
    }

    private Vector3Int ReadVector3Int(MessagePackObject o)
    {
        var l = o.AsList();
        return new Vector3Int(l[0].AsInt32(), l[1].AsInt32(), l[2].AsInt32());
    }

    private Quaternion ReadQuaternion(MessagePackObject o)
    {
        return Quaternion.Euler(ReadVector3(o));
    }

    private Color ReadColor(MessagePackObject o)
    {
        var l = o.AsList();
        if (l.Count == 4)
            return new Color(l[0].AsSingle(), l[1].AsSingle(), l[2].AsSingle(), l[3].AsSingle());
        else
            return new Color(l[0].AsSingle(), l[1].AsSingle(), l[2].AsSingle());
    }


    /* EmbeddedData search */

    public List<EmbeddedData> FindEmbeddedData(EmbeddedDataType type)
    {
        MessagePackObjectDictionary worldDict = worldObject.AsDictionary();
        CheckWorldValid(worldDict);

        List<EmbeddedData> dataList = new List<EmbeddedData>();
        try
        {
            foreach (var data in IterateEmbeddedData(worldDict, type))
                dataList.Add(data);
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("Error reading world file", e);
        }
        return dataList;
    }

    private IEnumerable<EmbeddedData> IterateEmbeddedData(
        MessagePackObjectDictionary worldDict, EmbeddedDataType type)
    {
        var typeString = type.ToString();
        foreach (var propList in IterateProperties(worldDict))
        {
            if (!propList[1].IsList)
                continue;
            var dataList = propList[1].AsList();
            if (dataList.Count >= 3 && dataList[1].UnderlyingType == typeof(string)
                && dataList[1].AsString() == typeString)
            {
                var name = dataList[0].AsString();
                var bytes = dataList[2].AsBinary();
                yield return new EmbeddedData(name, bytes, type);
            }
        }
    }

    private IEnumerable<IList<MessagePackObject>> IterateProperties(MessagePackObjectDictionary worldDict)
    {
        foreach (var propsDict in IterateWorldPropObjects(worldDict))
            if (propsDict.ContainsKey(FileKeys.PROPOBJ_PROPERTIES))
                foreach (var propObj in propsDict[FileKeys.PROPOBJ_PROPERTIES].AsList())
                    yield return propObj.AsList();
    }

    private IEnumerable<MessagePackObjectDictionary> IterateWorldPropObjects(MessagePackObjectDictionary worldDict)
    {
        if (worldDict.ContainsKey(FileKeys.WORLD_GLOBAL))
            yield return worldDict[FileKeys.WORLD_GLOBAL].AsDictionary();
        if (worldDict.ContainsKey(FileKeys.WORLD_SUBSTANCES))
            foreach (var subObj in worldDict[FileKeys.WORLD_SUBSTANCES].AsList())
                foreach (var propObj in IterateEntityPropObjects(subObj.AsDictionary()))
                    yield return propObj;
        if (worldDict.ContainsKey(FileKeys.WORLD_OBJECTS))
            foreach (var subObj in worldDict[FileKeys.WORLD_OBJECTS].AsList())
                foreach (var propObj in IterateEntityPropObjects(subObj.AsDictionary()))
                    yield return propObj;
    }

    private IEnumerable<MessagePackObjectDictionary> IterateEntityPropObjects(MessagePackObjectDictionary entityDict)
    {
        yield return entityDict;

        if (entityDict.ContainsKey(FileKeys.ENTITY_SENSOR))
            yield return entityDict[FileKeys.ENTITY_SENSOR].AsDictionary();
        if (entityDict.ContainsKey(FileKeys.ENTITY_BEHAVIORS))
            foreach (var behaviorObj in entityDict[FileKeys.ENTITY_BEHAVIORS].AsList())
                yield return behaviorObj.AsDictionary();
    }
}
