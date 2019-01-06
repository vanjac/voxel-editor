using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using MsgPack;
using System.Xml;
using System.Xml.Serialization;

public class MapFileWriter
{
    public const int VERSION = 8;
    private const int FILE_MIN_READER_VERSION = 8;

    private string fileName;

    public MapFileWriter(string fileName)
    {
        this.fileName = fileName;
    }

    public void Write(Transform cameraPivot, VoxelArray voxelArray)
    {
        if (voxelArray.IsEmpty())
        {
            Debug.Log("World is empty! File will not be written.");
            return;
        }

        var world = WriteWorld(cameraPivot, voxelArray);
        var worldObject = new MessagePackObject(world);

        string filePath = WorldFiles.GetFilePath(fileName);
        using (FileStream fileStream = File.Create(filePath))
        {
            var packer = Packer.Create(fileStream, PackerCompatibilityOptions.None);
            worldObject.PackToMessage(packer, null);
        }
    }

    private MessagePackObjectDictionary WriteCamera(Transform cameraPivot)
    {
        var camera = new MessagePackObjectDictionary();
        camera[FileKeys.CAMERA_PAN] = WriteVector3(cameraPivot.position);
        camera[FileKeys.CAMERA_ROTATE] = WriteQuaternion(cameraPivot.rotation);
        camera[FileKeys.CAMERA_SCALE] = cameraPivot.localScale.z;
        return camera;
    }

    private MessagePackObjectDictionary WriteWorld(Transform cameraPivot, VoxelArray voxelArray)
    {
        var world = new MessagePackObjectDictionary();

        world[FileKeys.WORLD_WRITER_VERSION] = VERSION;
        world[FileKeys.WORLD_MIN_READER_VERSION] = FILE_MIN_READER_VERSION;

        world[FileKeys.WORLD_CAMERA] = new MessagePackObject(WriteCamera(cameraPivot));

        var materialsList = new List<MessagePackObject>();
        var foundMaterials = new List<string>();
        var substancesList = new List<MessagePackObject>();
        var foundSubstances = new List<Substance>();

        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            foreach (VoxelFace face in voxel.faces)
            {
                AddMaterial(face.material, foundMaterials, materialsList);
                AddMaterial(face.overlay, foundMaterials, materialsList);
            }
            if (voxel.substance != null && !foundSubstances.Contains(voxel.substance))
            {
                foundSubstances.Add(voxel.substance);
                substancesList.Add(new MessagePackObject(WriteEntity(voxel.substance, false)));
            }
        }

        world[FileKeys.WORLD_MATERIALS] = new MessagePackObject(materialsList);
        if (foundSubstances.Count != 0)
            world[FileKeys.WORLD_SUBSTANCES] = new MessagePackObject(substancesList);
        world[FileKeys.WORLD_GLOBAL] = new MessagePackObject(WritePropertiesObject(voxelArray.world, false));

        var voxelsList = new List<MessagePackObject>();
        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            if (voxel.CanBeDeleted())
            {
                Debug.Log("Empty voxel found!");
                continue;
            }
            voxelsList.Add(WriteVoxel(voxel, foundMaterials, foundSubstances));
        }
        world[FileKeys.WORLD_VOXELS] = new MessagePackObject(voxelsList);

        var objectsList = new List<MessagePackObject>();
        foreach (ObjectEntity obj in voxelArray.IterateObjects())
            objectsList.Add(new MessagePackObject(WriteObjectEntity(obj, true)));
        if (objectsList.Count != 0)
            world[FileKeys.WORLD_OBJECTS] = new MessagePackObject(objectsList);

        return world;
    }

    void AddMaterial(Material material, List<string> foundMaterials, List<MessagePackObject> materialsList)
    {
        if (material == null)
            return;
        string name = material.name;
        if (foundMaterials.Contains(name))
            return;
        foundMaterials.Add(name);
        materialsList.Add(new MessagePackObject(WriteMaterial(material)));
    }

    private MessagePackObjectDictionary WriteMaterial(Material material)
    {
        var materialDict = new MessagePackObjectDictionary();
        if (ResourcesDirectory.IsCustomMaterial(material))
        {
            materialDict[FileKeys.MATERIAL_MODE] = ResourcesDirectory.GetCustomMaterialColorMode(material).ToString();
            materialDict[FileKeys.MATERIAL_COLOR] = WriteColor(material.color);
            materialDict[FileKeys.MATERIAL_ALPHA] = ResourcesDirectory.GetCustomMaterialIsTransparent(material);
        }
        else
        {
            materialDict[FileKeys.MATERIAL_NAME] = material.name;
        }
        return materialDict;
    }

    private MessagePackObjectDictionary WriteObjectEntity(ObjectEntity objectEntity, bool includeName)
    {
        var entityDict = WriteEntity(objectEntity, includeName);
        entityDict[FileKeys.OBJECT_POSITION] = WriteIntVector3(objectEntity.position);
        entityDict[FileKeys.OBJECT_ROTATION] = objectEntity.rotation;

        return entityDict;
    }

    private MessagePackObjectDictionary WriteEntity(Entity entity, bool includeName)
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

    private MessagePackObjectDictionary WritePropertiesObject(PropertiesObject obj, bool includeName)
    {
        var propsDict = new MessagePackObjectDictionary();

        if (includeName)
            propsDict[FileKeys.PROPOBJ_NAME] = obj.ObjectType().fullName;

        var propertiesList = new List<MessagePackObject>();
        foreach (Property prop in obj.Properties())
        {
            object value = prop.value;
            if (value == null)
            {
                Debug.Log(prop.name + " is null!");
                continue;
            }
            var propList = new List<MessagePackObject>();
            propList.Add(prop.name);
            var valueType = value.GetType();

            if (valueType == typeof(Material))
            {
                propList.Add(new MessagePackObject(WriteMaterial((Material)value)));
            }
            else // not a Material
            {
                XmlSerializer xmlSerializer;
                try
                {
                    xmlSerializer = new XmlSerializer(valueType);
                }
                catch (System.InvalidOperationException)
                {
                    Debug.Log(prop.name + " can't be serialized!");
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

    private MessagePackObject WriteVoxel(Voxel voxel, List<string> materials, List<Substance> substances)
    {
        var voxelList = new List<MessagePackObject>();
        voxelList.Add(WriteIntVector3(voxel.transform.position));

        var facesList = new List<MessagePackObject>();
        for (int faceI = 0; faceI < voxel.faces.Length; faceI++)
        {
            VoxelFace face = voxel.faces[faceI];
            if (face.IsEmpty())
                continue;
            facesList.Add(WriteFace(face, faceI, materials));
        }
        voxelList.Add(new MessagePackObject(facesList));

        if (voxel.substance != null)
            voxelList.Add(substances.IndexOf(voxel.substance));
        else
            voxelList.Add(-1);

        var edgesList = new List<MessagePackObject>();
        for (int edgeI = 0; edgeI < voxel.edges.Length; edgeI++)
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

    private MessagePackObject WriteFace(VoxelFace face, int faceI, List<string> materials)
    {
        var faceList = new List<MessagePackObject>();
        faceList.Add(faceI);
        if (face.material != null)
            faceList.Add(materials.IndexOf(face.material.name));
        else
            faceList.Add(-1);
        if (face.overlay != null)
            faceList.Add(materials.IndexOf(face.overlay.name));
        else
            faceList.Add(-1);
        faceList.Add(face.orientation);

        StripDataList(faceList, new bool[] {
            false, face.material == null, face.overlay == null, face.orientation == 0 });
        return new MessagePackObject(faceList);
    }

    private MessagePackObject WriteEdge(VoxelEdge edge, int edgeI)
    {
        var edgeList = new List<MessagePackObject>();
        edgeList.Add(edgeI);
        edgeList.Add(edge.bevel);
        return new MessagePackObject(edgeList);
    }

    // strip null values from the end of the list
    private void StripDataList(List<MessagePackObject> list, bool[] nullValues)
    {
        for (int i = nullValues.Length - 1; i >= 0; i--)
        {
            if (!nullValues[i])
                return;
            list.RemoveAt(i);
        }
    }

    private MessagePackObject WriteVector3(Vector3 v)
    {
        var l = new List<MessagePackObject>();
        l.Add(v.x);
        l.Add(v.y);
        l.Add(v.z);
        return new MessagePackObject(l);
    }

    private MessagePackObject WriteQuaternion(Quaternion v)
    {
        return WriteVector3(v.eulerAngles);
    }

    private MessagePackObject WriteIntVector3(Vector3 v)
    {
        var l = new List<MessagePackObject>();
        l.Add((int)(Mathf.RoundToInt(v.x)));
        l.Add((int)(Mathf.RoundToInt(v.y)));
        l.Add((int)(Mathf.RoundToInt(v.z)));
        return new MessagePackObject(l);
    }

    private MessagePackObject WriteColor(Color c)
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
