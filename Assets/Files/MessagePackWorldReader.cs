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

    private int fileWriterVersion;

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

    public void UseMessagePackObject(MessagePackObject obj)
    {
        worldObject = obj;
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
        fileWriterVersion = worldDict[FileKeys.WORLD_WRITER_VERSION].AsInt32();
        Debug.Log("Saved with writer version " + fileWriterVersion);
    }

    private void ReadWorld(MessagePackObjectDictionary world, Transform cameraPivot, VoxelArray voxelArray)
    {
        if (world.ContainsKey(FileKeys.WORLD_TYPE))
            voxelArray.type = (VoxelArray.WorldType)world[FileKeys.WORLD_TYPE].AsInt32();

        if (editor && cameraPivot != null && world.ContainsKey(FileKeys.WORLD_CAMERA))
            ReadCamera(world[FileKeys.WORLD_CAMERA].AsDictionary(), cameraPivot);

        voxelArray.customMaterials = new List<CustomMaterial>[]
            { new List<CustomMaterial>(), new List<CustomMaterial>() };

        var customBaseNames = new Dictionary<string, Material>();
        if (world.ContainsKey(FileKeys.WORLD_CUSTOM_BASE_MATERIALS))
        {
            foreach (var texObj in world[FileKeys.WORLD_CUSTOM_BASE_MATERIALS].AsList())
                voxelArray.customMaterials[(int)PaintLayer.BASE].Add(
                    ReadCustomMaterial(texObj.AsDictionary(), customBaseNames, PaintLayer.BASE));
        }

        var customOverlayNames = new Dictionary<string, Material>();
        if (world.ContainsKey(FileKeys.WORLD_CUSTOM_OVERLAY_MATERIALS))
        {
            foreach (var texObj in world[FileKeys.WORLD_CUSTOM_OVERLAY_MATERIALS].AsList())
                voxelArray.customMaterials[(int)PaintLayer.OVERLAY].Add(
                    ReadCustomMaterial(texObj.AsDictionary(), customOverlayNames, PaintLayer.OVERLAY));
        }

        var bases = new List<Material>();
        if (world.ContainsKey(FileKeys.WORLD_BASE_MATERIALS))
        {
            foreach (var matObj in world[FileKeys.WORLD_BASE_MATERIALS].AsList())
                bases.Add(ReadMaterial(matObj.AsDictionary(), customBaseNames, PaintLayer.BASE));
        }

        var overlays = new List<Material>();
        if (world.ContainsKey(FileKeys.WORLD_OVERLAY_MATERIALS))
        {
            foreach (var matObj in world[FileKeys.WORLD_OVERLAY_MATERIALS].AsList())
                overlays.Add(ReadMaterial(matObj.AsDictionary(), customOverlayNames, PaintLayer.OVERLAY));
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
        {
            ReadPropertiesObject(world[FileKeys.WORLD_GLOBAL].AsDictionary(), voxelArray.world);
            // the new skybox shader makes ambient light for this sky a lot brighter
            if (fileWriterVersion <= 10 && RenderSettings.skybox.name == "sky5X3")
                RenderSettings.ambientIntensity *= 0.67f;
        }

        if (world.ContainsKey(FileKeys.WORLD_VOXELS))
        {
            foreach (var voxelObj in world[FileKeys.WORLD_VOXELS].AsList())
                ReadVoxel(voxelObj, voxelArray, bases, overlays, substances);
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
                ReadObjectEntity(objDict, obj, bases, overlays);
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

    private CustomMaterial ReadCustomMaterial(MessagePackObjectDictionary texDict,
        Dictionary<string, Material> customMaterialNames, PaintLayer layer)
    {
        CustomMaterial customMat = new CustomMaterial(layer);
        ReadPropertiesObject(texDict, customMat);
        if (texDict.ContainsKey(FileKeys.CUSTOM_MATERIAL_NAME))
        {
            customMat.material.name = texDict[FileKeys.CUSTOM_MATERIAL_NAME].AsString();
            if (customMaterialNames != null)
                customMaterialNames[customMat.material.name] = customMat.material;
        }
        return customMat;
    }

    private Material ReadMaterial(MessagePackObjectDictionary matDict,
        Dictionary<string, Material> customMaterialNames, PaintLayer layer)
    {
        string name;

        if (matDict.ContainsKey(FileKeys.MATERIAL_NAME))
        {
            name = matDict[FileKeys.MATERIAL_NAME].AsString();
        }
        else if (matDict.ContainsKey(FileKeys.MATERIAL_MODE))  // version 9 and earlier
        {
            name = matDict[FileKeys.MATERIAL_MODE].AsString();
            // ignore MATERIAL_ALPHA key, it's usually wrong
            if (matDict.ContainsKey(FileKeys.MATERIAL_COLOR))
                if (ReadColor(matDict[FileKeys.MATERIAL_COLOR]).a < 1)
                    layer = PaintLayer.OVERLAY;
            if (layer == PaintLayer.OVERLAY)
                name += "_overlay";
        }
        else
        {
            warnings.Add("Error reading material");
            return ReadWorldFile.MissingMaterial(layer);
        }

        Material mat;
        bool isCustom = customMaterialNames != null && customMaterialNames.ContainsKey(name);
        if (isCustom)
            mat = customMaterialNames[name];
        else
            mat = ResourcesDirectory.FindMaterial(name, editor);
        if (mat == null)
        {
            warnings.Add("Unrecognized material: " + name);
            return ReadWorldFile.MissingMaterial(layer);
        }
        if (!isCustom && matDict.ContainsKey(FileKeys.MATERIAL_COLOR))
        {
            // custom materials can't have colors
            // TODO if color style = PAINT then replace with solid color matching shader/footsteps
            // var colorStyle = ResourcesDirectory.ColorStyle.TINT;
            // if (matDict.ContainsKey(FileKeys.MATERIAL_COLOR_STYLE))
            //     Enum.TryParse(matDict[FileKeys.MATERIAL_COLOR_STYLE].AsString(), out colorStyle);

            Color color = ReadColor(matDict[FileKeys.MATERIAL_COLOR]);
            if (color != mat.color)
            {
                // TODO !!!
                // mat = ResourcesDirectory.InstantiateMaterial(mat);
                // mat.color = color;
            }
        }
        return mat;
    }

    private void ReadObjectEntity(MessagePackObjectDictionary entityDict, ObjectEntity objectEntity,
        List<Material> bases, List<Material> overlays)
    {
        ReadEntity(entityDict, objectEntity);
        if (entityDict.ContainsKey(FileKeys.OBJECT_POSITION))
            objectEntity.position = ReadVector3Int(entityDict[FileKeys.OBJECT_POSITION]);
        if (entityDict.ContainsKey(FileKeys.OBJECT_ROTATION))
            objectEntity.rotation = entityDict[FileKeys.OBJECT_ROTATION].AsSingle();
        if (entityDict.ContainsKey(FileKeys.OBJECT_PAINT))
            objectEntity.paint = ReadFace(entityDict[FileKeys.OBJECT_PAINT], out int _,
                bases, overlays);
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
                if (newBehavior is LightBehavior light && light.halo)
                {
                    // convert halo from version 10 and earlier
                    HaloBehavior halo = new HaloBehavior();
                    halo.condition = light.condition;
                    halo.targetEntity = light.targetEntity;
                    halo.targetEntityIsActivator = light.targetEntityIsActivator;
                    if (PropertiesObjectType.GetProperty(light, "siz") is float size)
                        PropertiesObjectType.SetProperty(halo, "siz", size);
                    if (PropertiesObjectType.GetProperty(light, "col") is Color color
                            && PropertiesObjectType.GetProperty(light, "int") is float intensity)
                        PropertiesObjectType.SetProperty(halo, "col",
                            color * intensity / HaloComponent.INTENSITY);
                    entity.behaviors.Add(halo);
                    light.halo = false;
                }
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

                prop.setter(ReadPropertyValue(propList, propType));
            }
        }
    }

    private object ReadPropertyValue(IList<MessagePackObject> propList, System.Type propType)
    {
        if (propType == typeof(VoxelFaceLayer))
        {
            // skip equality check
            // TODO!!!
            return new VoxelFaceLayer {
                material = ReadMaterial(propList[1].AsDictionary(), null, PaintLayer.BASE),
                color = Color.white };
        }
        else if (propType == typeof(Texture2D))
        {
            Texture2D tex = new Texture2D(2, 2);
            if (!ImageConversion.LoadImage(tex, propList[1].AsBinary()))
                warnings.Add("Error reading texture data");
            return tex;
        }
        else if (propType == typeof(EmbeddedData))
        {
            var dataList = propList[1].AsList();
            var name = dataList[0].AsString();
            var type = (EmbeddedDataType)System.Enum.Parse(typeof(EmbeddedDataType), dataList[1].AsString());
            var bytes = dataList[2].AsBinary();
            return new EmbeddedData(name, bytes, type);
        }
        else // not a special type
        {
            string valueString = propList[1].AsString();
            XmlSerializer xmlSerializer = new XmlSerializer(propType);
            using (var textReader = new StringReader(valueString))
            {
                // skip equality check. important if this is an EntityReference,
                // since EntityReference.Equals gets the entity which may not exist yet
                return xmlSerializer.Deserialize(textReader);
            }
        }
    }

    private void ReadVoxel(MessagePackObject voxelObj, VoxelArray voxelArray,
        List<Material> bases, List<Material> overlays, List<Substance> substances)
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
                VoxelFace face = ReadFace(faceObj, out int faceI, bases, overlays);
                if (faceI != -1)
                    voxel.faces[faceI] = face;
            }
        }

        if (voxelList.Count >= 3 && voxelList[2].AsInt32() != -1)
            voxel.substance = substances[voxelList[2].AsInt32()];

        if (voxelList.Count >= 4)
            foreach (var edgeObj in voxelList[3].AsList())
                ReadEdge(edgeObj, voxel);

        voxel.UpdateVoxel();
    }

    private VoxelFace ReadFace(MessagePackObject faceObj, out int faceI,
        List<Material> bases, List<Material> overlays)
    {
        VoxelFace face = new VoxelFace();
        face.Clear();
        var faceList = faceObj.AsList();
        if (faceList.Count >= 1)
            faceI = faceList[0].AsInt32();
        else
            faceI = -1;
        if (faceList.Count >= 2 && faceList[1].AsInt32() != -1)
            face.baseLayer.material = bases[faceList[1].AsInt32()];
        if (faceList.Count >= 3 && faceList[2].AsInt32() != -1)
            face.overlay.material = overlays[faceList[2].AsInt32()];
        if (faceList.Count >= 4)
            face.orientation = faceList[3].AsByte();
        return face;
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

    private Vector2 ReadVector2(MessagePackObject o)
    {
        var l = o.AsList();
        return new Vector2(l[0].AsSingle(), l[1].AsSingle());
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
                yield return (EmbeddedData)ReadPropertyValue(propList, typeof(EmbeddedData));
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
        if (worldDict.ContainsKey(FileKeys.WORLD_CUSTOM_BASE_MATERIALS))
            foreach (var matObj in worldDict[FileKeys.WORLD_CUSTOM_BASE_MATERIALS].AsList())
                yield return matObj.AsDictionary();
        if (worldDict.ContainsKey(FileKeys.WORLD_CUSTOM_OVERLAY_MATERIALS))
            foreach (var matObj in worldDict[FileKeys.WORLD_CUSTOM_OVERLAY_MATERIALS].AsList())
                yield return matObj.AsDictionary();
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

    private IList<MessagePackObject> FindProperty(MessagePackObjectDictionary propsDict, string name)
    {
        if (propsDict.ContainsKey(FileKeys.PROPOBJ_PROPERTIES))
        {
            foreach (var propObj in propsDict[FileKeys.PROPOBJ_PROPERTIES].AsList())
            {
                var propList = propObj.AsList();
                if (propList.Count >= 2 && propList[0] == name)
                    return propList;
            }
        }
        return null;
    }

    public List<string> GetCustomMaterialCategories(PaintLayer layer)
    {
        // TODO so much copy/pasting!
        MessagePackObjectDictionary worldDict = worldObject.AsDictionary();
        CheckWorldValid(worldDict);

        string key = layer == PaintLayer.OVERLAY ?
            FileKeys.WORLD_CUSTOM_OVERLAY_MATERIALS : FileKeys.WORLD_CUSTOM_BASE_MATERIALS;
        var categories = new SortedSet<string>();
        try
        {
            if (worldDict.ContainsKey(key))
            {
                foreach (var matObj in worldDict[key].AsList())
                {
                    var matDict = matObj.AsDictionary();
                    var catProp = FindProperty(matDict, "cat");
                    if (catProp != null)
                        categories.Add((string)ReadPropertyValue(catProp, typeof(string)));
                }
            }
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("Error reading world file", e);
        }
        return new List<string>(categories);
    }

    public List<CustomMaterial> FindCustomMaterials(PaintLayer layer, string category)
    {
        // copied from FindEmbeddedData
        MessagePackObjectDictionary worldDict = worldObject.AsDictionary();
        CheckWorldValid(worldDict);

        string key = layer == PaintLayer.OVERLAY ?
            FileKeys.WORLD_CUSTOM_OVERLAY_MATERIALS : FileKeys.WORLD_CUSTOM_BASE_MATERIALS;
        var matList = new List<CustomMaterial>();
        try
        {
            if (worldDict.ContainsKey(key))
            {
                foreach (var matObj in worldDict[key].AsList())
                {
                    var matDict = matObj.AsDictionary();
                    var catProp = FindProperty(matDict, "cat");
                    if (catProp != null && (string)ReadPropertyValue(catProp, typeof(string)) == category)
                        matList.Add(ReadCustomMaterial(matObj.AsDictionary(), null, layer));
                }
            }
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("Error reading world file", e);
        }
        return matList;
    }
}
