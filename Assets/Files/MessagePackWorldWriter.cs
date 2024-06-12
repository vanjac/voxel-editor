using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MsgPack;
using System.Xml;
using System.Xml.Serialization;

public class MessagePackWorldWriter
{
    public const int VERSION = 11;
    private const int FILE_MIN_READER_VERSION = 10;

    public static void Write(string filePath, Transform cameraPivot, VoxelArray voxelArray)
    {
        Debug.Log("Writing MessagePack file " + filePath);

        var world = WriteWorld(cameraPivot, voxelArray);
        var worldObject = new MessagePackObject(world);

        using (FileStream fileStream = File.Create(filePath))
        {
            fileStream.WriteByte((byte)'m');
            var packer = Packer.Create(fileStream, PackerCompatibilityOptions.None);
            worldObject.PackToMessage(packer, null);
        }
    }

    private static MessagePackObjectDictionary WriteWorld(Transform cameraPivot, VoxelArray voxelArray)
    {
        var world = new MessagePackObjectDictionary();

        world[FileKeys.WORLD_WRITER_VERSION] = VERSION;
        world[FileKeys.WORLD_MIN_READER_VERSION] = FILE_MIN_READER_VERSION;

        world[FileKeys.WORLD_TYPE] = (int)voxelArray.type;

        world[FileKeys.WORLD_CAMERA] = new MessagePackObject(WriteCamera(cameraPivot));

        var customMaterialsList = new List<MessagePackObject>();
        foreach (Material mat in voxelArray.customMaterials)
            customMaterialsList.Add(new MessagePackObject(WriteCustomTexture(mat, false)));
        if (customMaterialsList.Count != 0)
            world[FileKeys.WORLD_CUSTOM_MATERIALS] = new MessagePackObject(customMaterialsList);

        var customOverlaysList = new List<MessagePackObject>();
        foreach (Material mat in voxelArray.customOverlays)
            customOverlaysList.Add(new MessagePackObject(WriteCustomTexture(mat, true)));
        if (customOverlaysList.Count != 0)
            world[FileKeys.WORLD_CUSTOM_OVERLAYS] = new MessagePackObject(customOverlaysList);

        var materialsList = new List<MessagePackObject>();
        // map to indices in materials list
        var foundMaterials = new Dictionary<Material, int>();
        var overlaysList = new List<MessagePackObject>();
        var foundOverlays = new Dictionary<Material, int>();
        var substancesList = new List<MessagePackObject>();
        var foundSubstances = new Dictionary<Substance, int>();

        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            foreach (VoxelFace face in voxel.faces)
            {
                AddMaterial(face.material, foundMaterials, materialsList);
                AddMaterial(face.overlay, foundOverlays, overlaysList);
            }
            if (voxel.substance != null && !foundSubstances.ContainsKey(voxel.substance))
            {
                foundSubstances[voxel.substance] = substancesList.Count;
                substancesList.Add(new MessagePackObject(WriteEntity(voxel.substance, false)));
            }
        }
        foreach (ObjectEntity obj in voxelArray.IterateObjects())
        {
            AddMaterial(obj.paint.material, foundMaterials, materialsList);
            AddMaterial(obj.paint.overlay, foundOverlays, overlaysList);
        }

        world[FileKeys.WORLD_MATERIALS] = new MessagePackObject(materialsList);
        world[FileKeys.WORLD_OVERLAYS] = new MessagePackObject(overlaysList);
        if (foundSubstances.Count != 0)
            world[FileKeys.WORLD_SUBSTANCES] = new MessagePackObject(substancesList);
        world[FileKeys.WORLD_GLOBAL] = new MessagePackObject(WritePropertiesObject(voxelArray.world, false));

        var voxelsList = new List<MessagePackObject>();
        foreach (var (pos, voxel) in voxelArray.IterateVoxelPairs())
        {
            if (voxel.IsEmpty())
            {
                Debug.LogWarning("Empty voxel found!");
                continue;
            }
            voxelsList.Add(WriteVoxel(pos, voxel, foundMaterials, foundOverlays, foundSubstances));
        }
        world[FileKeys.WORLD_VOXELS] = new MessagePackObject(voxelsList);

        var objectsList = new List<MessagePackObject>();
        foreach (ObjectEntity obj in voxelArray.IterateObjects())
            objectsList.Add(new MessagePackObject(WriteObjectEntity(obj, foundMaterials, foundOverlays)));
        if (objectsList.Count != 0)
            world[FileKeys.WORLD_OBJECTS] = new MessagePackObject(objectsList);

        return world;
    }

    private static MessagePackObjectDictionary WriteCamera(Transform cameraPivot)
    {
        var camera = new MessagePackObjectDictionary();
        camera[FileKeys.CAMERA_PAN] = WriteVector3(cameraPivot.position);
        camera[FileKeys.CAMERA_ROTATE] = WriteQuaternion(cameraPivot.rotation);
        camera[FileKeys.CAMERA_SCALE] = cameraPivot.localScale.z;
        return camera;
    }

    private static MessagePackObjectDictionary WriteCustomTexture(Material material, bool overlay)
    {
        CustomTexture customTex = new CustomTexture(material, overlay);
        var materialDict = WritePropertiesObject(customTex, false);
        materialDict[FileKeys.CUSTOM_MATERIAL_NAME] = material.name;
        return materialDict;
    }

    private static void AddMaterial(Material material, Dictionary<Material, int> foundMaterials,
        List<MessagePackObject> materialsList)
    {
        if (material == null)
            return;
        if (foundMaterials.ContainsKey(material))
            return;
        foundMaterials[material] = materialsList.Count;
        materialsList.Add(new MessagePackObject(WriteMaterial(material)));
    }

    private static MessagePackObjectDictionary WriteMaterial(Material material)
    {
        var materialDict = new MessagePackObjectDictionary();
        materialDict[FileKeys.MATERIAL_NAME] = material.name;
        string colorProp = CustomTexture.IsCustomTexture(material) ? null
            : ResourcesDirectory.MaterialColorProperty(material);
        if (colorProp != null)
        {
            materialDict[FileKeys.MATERIAL_COLOR] = WriteColor(material.GetColor(colorProp));
            materialDict[FileKeys.MATERIAL_COLOR_STYLE]
                = ResourcesDirectory.GetMaterialColorStyle(material).ToString();
        }
        return materialDict;
    }

    private static MessagePackObjectDictionary WriteObjectEntity(ObjectEntity objectEntity,
        Dictionary<Material, int> materials, Dictionary<Material, int> overlays)
    {
        var entityDict = WriteEntity(objectEntity, true);
        entityDict[FileKeys.OBJECT_POSITION] = WriteVector3Int(objectEntity.position);
        entityDict[FileKeys.OBJECT_ROTATION] = objectEntity.rotation;
        entityDict[FileKeys.OBJECT_PAINT] = WriteFace(objectEntity.paint, 0, materials, overlays);

        return entityDict;
    }

    private static MessagePackObjectDictionary WriteEntity(Entity entity, bool includeName)
    {
        var entityDict = WritePropertiesObject(entity, includeName);

        if (entity.sensor != null)
            entityDict[FileKeys.ENTITY_SENSOR] = new MessagePackObject(WritePropertiesObject(entity.sensor, true));

        if (entity.behaviors.Count != 0)
        {
            var behaviorsList = new List<MessagePackObject>();
            foreach (EntityBehavior behavior in entity.behaviors)
            {
                var behaviorDict = WritePropertiesObject(behavior, true);
                behaviorsList.Add(new MessagePackObject(behaviorDict));
            }
            entityDict[FileKeys.ENTITY_BEHAVIORS] = new MessagePackObject(behaviorsList);
        }

        if (entity.guid != System.Guid.Empty)
            entityDict[FileKeys.ENTITY_ID] = entity.guid.ToString(); // can be referenced by EntityReference properties

        return entityDict;
    }

    private static MessagePackObjectDictionary WritePropertiesObject(PropertiesObject obj, bool includeName)
    {
        var propsDict = new MessagePackObjectDictionary();

        if (includeName)
            propsDict[FileKeys.PROPOBJ_NAME] = obj.ObjectType.fullName;

        var propertiesList = new List<MessagePackObject>();
        foreach (Property prop in obj.Properties())
        {
            object value = prop.value;
            if (value == null)
            {
                Debug.Log(prop.id + " is null!");
                continue;
            }
            var propList = new List<MessagePackObject>();
            propList.Add(prop.id);
            var valueType = value.GetType();

            if (valueType == typeof(Material))
            {
                propList.Add(new MessagePackObject(WriteMaterial((Material)value)));
            }
            else if (valueType == typeof(EmbeddedData))
            {
                var embeddedData = (EmbeddedData)value;
                var dataList = new List<MessagePackObject>();
                dataList.Add(embeddedData.name);
                dataList.Add(embeddedData.type.ToString());
                dataList.Add(embeddedData.bytes);
                propList.Add(new MessagePackObject(dataList));
            }
            else if (valueType == typeof(Texture2D))
            {
                propList.Add(ImageConversion.EncodeToPNG((Texture2D)value));
            }
            else // not a special type
            {
                XmlSerializer xmlSerializer;
                try
                {
                    xmlSerializer = new XmlSerializer(valueType);
                }
                catch (System.InvalidOperationException)
                {
                    Debug.Log(prop.id + " can't be serialized!");
                    continue;
                }
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", ""); // skip xsi/xsd namespaces: https://stackoverflow.com/a/935749
                string valueString;
                // https://stackoverflow.com/a/2434558
                // https://stackoverflow.com/a/5414665
                using (var textWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(textWriter,
                    new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
                {
                    xmlSerializer.Serialize(xmlWriter, value, ns);
                    valueString = textWriter.ToString();
                }
                propList.Add(valueString);
            }

            if (prop.explicitType)
                propList.Add(value.GetType().FullName);
            propertiesList.Add(new MessagePackObject(propList));
        }
        propsDict[FileKeys.PROPOBJ_PROPERTIES] = new MessagePackObject(propertiesList);

        return propsDict;
    }

    private static MessagePackObject WriteVoxel(Vector3Int position, Voxel voxel,
        Dictionary<Material, int> materials, Dictionary<Material, int> overlays,
        Dictionary<Substance, int> substances)
    {
        var voxelList = new List<MessagePackObject>();
        voxelList.Add(WriteVector3Int(position));

        var facesList = new List<MessagePackObject>();
        for (int faceI = 0; faceI < Voxel.NUM_FACES; faceI++)
        {
            VoxelFace face = voxel.faces[faceI];
            if (face.IsEmpty())
                continue;
            facesList.Add(WriteFace(face, faceI, materials, overlays));
        }
        voxelList.Add(new MessagePackObject(facesList));

        if (voxel.substance != null)
            voxelList.Add(substances[voxel.substance]);
        else
            voxelList.Add(-1);

        var edgesList = new List<MessagePackObject>();
        for (int edgeI = 0; edgeI < Voxel.NUM_EDGES; edgeI++)
        {
            VoxelEdge edge = voxel.edges[edgeI];
            if (!edge.hasBevel)
                continue;
            edgesList.Add(WriteEdge(edge, edgeI));
        }
        voxelList.Add(new MessagePackObject(edgesList));

        StripDataList(voxelList,
            new bool[] { false, facesList.Count == 0, voxel.substance == null, edgesList.Count == 0 });
        return new MessagePackObject(voxelList);
    }

    private static MessagePackObject WriteFace(VoxelFace face, int faceI,
        Dictionary<Material, int> materials, Dictionary<Material, int> overlays)
    {
        var faceList = new List<MessagePackObject>();
        faceList.Add(faceI);
        if (face.material != null)
            faceList.Add(materials[face.material]);
        else
            faceList.Add(-1);
        if (face.overlay != null)
            faceList.Add(overlays[face.overlay]);
        else
            faceList.Add(-1);
        faceList.Add(face.orientation);

        StripDataList(faceList, new bool[] {
            false, face.material == null, face.overlay == null, face.orientation == 0 });
        return new MessagePackObject(faceList);
    }

    private static MessagePackObject WriteEdge(VoxelEdge edge, int edgeI)
    {
        var edgeList = new List<MessagePackObject>();
        edgeList.Add(edgeI);
        edgeList.Add(edge.bevel);
        return new MessagePackObject(edgeList);
    }

    // strip null values from the end of the list
    private static void StripDataList(List<MessagePackObject> list, bool[] nullValues)
    {
        for (int i = nullValues.Length - 1; i >= 0; i--)
        {
            if (!nullValues[i])
                return;
            list.RemoveAt(i);
        }
    }

    private static MessagePackObject WriteVector2(Vector2 v)
    {
        var l = new List<MessagePackObject>();
        l.Add(v.x);
        l.Add(v.y);
        return new MessagePackObject(l);
    }

    private static MessagePackObject WriteVector3(Vector3 v)
    {
        var l = new List<MessagePackObject>();
        l.Add(v.x);
        l.Add(v.y);
        l.Add(v.z);
        return new MessagePackObject(l);
    }

    private static MessagePackObject WriteQuaternion(Quaternion v) => WriteVector3(v.eulerAngles);

    private static MessagePackObject WriteVector3Int(Vector3Int v)
    {
        var l = new List<MessagePackObject>();
        l.Add(v.x);
        l.Add(v.y);
        l.Add(v.z);
        return new MessagePackObject(l);
    }

    private static MessagePackObject WriteColor(Color c)
    {
        var l = new List<MessagePackObject>();
        l.Add(c.r);
        l.Add(c.g);
        l.Add(c.b);
        if (c.a != 1)
            l.Add(c.a);
        return new MessagePackObject(l);
    }
}
