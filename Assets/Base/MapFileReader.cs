﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using MsgPack;
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

    private const string ERROR_INVALID_FILE = "Invalid world file";

    private string fileName;
    private int fileWriterVersion;
    private Material missingMaterial; // material to be used when material can't be created

    private List<string> warnings = new List<string>();
    private bool editor;

    public MapFileReader(string fileName)
    {
        this.fileName = fileName;
    }

    // return warnings
    public List<string> Read(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        this.editor = editor;
        if (missingMaterial == null)
        {
            // allowTransparency is true in case the material is used for an overlay, so the alpha value can be adjusted
            missingMaterial = ResourcesDirectory.MakeCustomMaterial(ColorMode.UNLIT, true);
            missingMaterial.color = Color.magenta;
        }

        MessagePackObject world;

        string filePath = WorldFiles.GetFilePath(fileName);
        try
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
            {
                world = Unpacking.UnpackObject(fileStream);
            }
        }
        catch (UnpackException e)
        {
            throw new MapReadException(ERROR_INVALID_FILE, e);
        }
        catch (MessageTypeException e)
        {
            throw new MapReadException(ERROR_INVALID_FILE, e);
        }
        catch (Exception e)
        {
            throw new MapReadException("An error occurred while reading the file", e);
        }
        if (!world.IsDictionary)
            throw new MapReadException(ERROR_INVALID_FILE);
        MessagePackObjectDictionary worldDict = world.AsDictionary();
        if (!worldDict.ContainsKey(FileKeys.WORLD_WRITER_VERSION)
            || !worldDict.ContainsKey(FileKeys.WORLD_MIN_READER_VERSION))
            throw new MapReadException(ERROR_INVALID_FILE);

        if (worldDict[FileKeys.WORLD_MIN_READER_VERSION].AsInt32() > VERSION)
        {
            throw new MapReadException("This world file requires a newer version of the app");
        }
        fileWriterVersion = worldDict[FileKeys.WORLD_WRITER_VERSION].AsInt32();

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

    private void ReadWorld(MessagePackObjectDictionary world, Transform cameraPivot, VoxelArray voxelArray)
    {
        if (editor && cameraPivot != null && world.ContainsKey(FileKeys.WORLD_CAMERA))
            ReadCamera(world[FileKeys.WORLD_CAMERA].AsDictionary(), cameraPivot);

        var materials = new List<Material>();
        if (world.ContainsKey(FileKeys.WORLD_MATERIALS))
        {
            foreach (var matObj in world[FileKeys.WORLD_MATERIALS].AsList())
                materials.Add(ReadMaterial(matObj.AsDictionary()));
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
                ReadVoxel(voxelObj, voxelArray, materials, substances);
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

    private Material ReadMaterial(MessagePackObjectDictionary matDict)
    {
        if (matDict.ContainsKey(FileKeys.MATERIAL_NAME))
        {
            string name = matDict[FileKeys.MATERIAL_NAME].AsString();
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
            warnings.Add("Unrecognized material: " + name);
            return missingMaterial;
        }
        else if (matDict.ContainsKey(FileKeys.MATERIAL_MODE))
        {
            ColorMode mode = (ColorMode)System.Enum.Parse(typeof(ColorMode), matDict[FileKeys.MATERIAL_MODE].AsString());
            if (matDict.ContainsKey(FileKeys.MATERIAL_COLOR))
            {
                Color color = ReadColor(matDict[FileKeys.MATERIAL_COLOR]);
                bool alpha = color.a != 1;
                if (matDict.ContainsKey(FileKeys.MATERIAL_ALPHA))
                    alpha = matDict[FileKeys.MATERIAL_ALPHA].AsBoolean(); // new with version 4
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
            return missingMaterial;
        }
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
                string name = propList[0].AsString();

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
                    warnings.Add("Unrecognized property: " + name);
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
                    prop.setter(ReadMaterial(propList[1].AsDictionary()));
                }
                else
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
        List<Material> materials, List<Substance> substances)
    {
        var voxelList = voxelObj.AsList();
        if (voxelList.Count == 0)
            return;

        Vector3 position = ReadVector3(voxelList[0]);
        Voxel voxel = null;
        if (!editor)
            // slightly faster -- doesn't add to octree
            voxel = voxelArray.InstantiateVoxel(position);
        else
            voxel = voxelArray.VoxelAt(position, true);

        if (voxelList.Count >= 2)
        {
            foreach (var faceObj in voxelList[1].AsList())
            {
                ReadFace(faceObj, voxel, materials);
            }
        }

        if (voxelList.Count >= 3 && voxelList[2].AsInt32() != -1)
            voxel.substance = substances[voxelList[2].AsInt32()];

        if (voxelList.Count >= 4)
            foreach (var edgeObj in voxelList[3].AsList())
                ReadEdge(edgeObj, voxel);

        voxel.UpdateVoxel();
    }

    private void ReadFace(MessagePackObject faceObj, Voxel voxel, List<Material> materials)
    {
        var faceList = faceObj.AsList();
        if (faceList.Count == 0)
            return;
        int faceI = faceList[0].AsInt32();
        if (faceList.Count >= 2 && faceList[1].AsInt32() != -1)
            voxel.faces[faceI].material = materials[faceList[1].AsInt32()];
        if (faceList.Count >= 3 && faceList[2].AsInt32() != -1)
            voxel.faces[faceI].overlay = materials[faceList[2].AsInt32()];
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
}
