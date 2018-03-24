using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using SimpleJSON;
using System.Xml;
using System.Xml.Serialization;

public class MapFileWriter
{
    public const int VERSION = 1;
    private const int FILE_MIN_READER_VERSION = 1;

    private string fileName;

    public MapFileWriter(string fileName)
    {
        this.fileName = fileName;
    }

    public void Write(Transform cameraPivot, VoxelArray voxelArray)
    {
        JSONObject root = new JSONObject();

        root["writerVersion"] = VERSION;
        root["minReaderVersion"] = FILE_MIN_READER_VERSION;

        JSONObject camera = new JSONObject();
        camera["pan"] = WriteVector3(cameraPivot.position);
        camera["rotate"] = WriteQuaternion(cameraPivot.rotation);
        camera["scale"] = cameraPivot.localScale.z;
        root["camera"] = camera;

        root["world"] = WriteWorld(voxelArray);

        string filePath = Application.persistentDataPath + "/" + fileName + ".json";
        using (FileStream fileStream = File.Create(filePath))
        {
            using (var sw = new StreamWriter(fileStream))
            {
                sw.Write(root.ToString());
                sw.Flush();
            }
        }
    }

    private JSONObject WriteWorld(VoxelArray voxelArray)
    {
        JSONObject world = new JSONObject();

        JSONArray materialsArray = new JSONArray();
        var foundMaterials = new List<string>();
        JSONArray substancesArray = new JSONArray();
        var foundSubstances = new List<Substance>();

        AddMaterial(RenderSettings.skybox, foundMaterials, materialsArray);
        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            foreach (VoxelFace face in voxel.faces)
            {
                AddMaterial(face.material, foundMaterials, materialsArray);
                AddMaterial(face.overlay, foundMaterials, materialsArray);
            }
            if (voxel.substance != null && !foundSubstances.Contains(voxel.substance))
            {
                foundSubstances.Add(voxel.substance);
                substancesArray[-1] = WriteEntity(voxel.substance, false);
            }
        }

        world["materials"] = materialsArray;
        if (foundSubstances.Count != 0)
            world["substances"] = substancesArray;
        world["lighting"] = WriteLighting(foundMaterials);
        world["map"] = WriteMap(voxelArray, foundMaterials, foundSubstances);
        world["player"] = WriteObjectEntity(voxelArray.playerObject, false);

        return world;
    }

    void AddMaterial(Material material, List<string> foundMaterials, JSONArray materialsArray)
    {
        if (material == null)
            return;
        string name = material.name;
        if (foundMaterials.Contains(name))
            return;
        foundMaterials.Add(name);
        materialsArray[-1] = WriteMaterial(material);
    }

    private JSONObject WriteMaterial(Material material)
    {
        JSONObject materialObject = new JSONObject();
        if (ResourcesDirectory.IsCustomMaterial(material))
        {
            // TODO: allow more properties in the future
            materialObject["shader"] = material.shader.name;
            materialObject["color"] = WriteColor(material.color);
        }
        else
        {
            materialObject["name"] = material.name;
        }
        return materialObject;
    }

    private JSONObject WriteObjectEntity(ObjectEntity objectEntity, bool includeName)
    {
        JSONObject entityObject = WriteEntity(objectEntity, includeName);
        entityObject["at"] = WriteIntVector3(objectEntity.position);

        return entityObject;
    }

    private JSONObject WriteEntity(Entity entity, bool includeName)
    {
        JSONObject entityObject = WritePropertiesObject(entity, includeName);

        if (entity.sensor != null)
            entityObject["sensor"] = WritePropertiesObject(entity.sensor, true);

        if (entity.behaviors.Count != 0) {
            JSONArray behaviorsArray = new JSONArray();
            foreach (EntityBehavior behavior in entity.behaviors)
                behaviorsArray[-1] = WritePropertiesObject(behavior, true);
            entityObject["behaviors"] = behaviorsArray;
        }

        if (entity.guid != System.Guid.Empty)
            entityObject["id"] = entity.guid.ToString(); // can be referenced by EntityReference properties

        return entityObject;
    }

    private JSONObject WritePropertiesObject(PropertiesObject obj, bool includeName)
    {
        JSONObject propsObject = new JSONObject();

        if (includeName)
            propsObject["name"] = obj.ObjectType().fullName;

        JSONArray propertiesArray = new JSONArray();
        foreach (Property prop in obj.Properties())
        {
            // https://stackoverflow.com/a/2434558
            // https://stackoverflow.com/a/5414665
            object value = prop.value;
            XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", ""); // skip xsi/xsd namespaces: https://stackoverflow.com/a/935749
            string valueString;
            using (var textWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(textWriter,
                new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
            {
                xmlSerializer.Serialize(xmlWriter, value, ns);
                valueString = textWriter.ToString();
            }
            JSONArray propArray = new JSONArray();
            propArray[0] = prop.name;
            propArray[1] = valueString;
            if (prop.explicitType)
                propArray[2] = value.GetType().FullName;
            propertiesArray[-1] = propArray;
        }
        propsObject["properties"] = propertiesArray;

        return propsObject;
    }

    private JSONObject WriteLighting(List<string> materials)
    {
        JSONObject lighting = new JSONObject();
        lighting["sky"].AsInt = materials.IndexOf(RenderSettings.skybox.name);
        lighting["ambientIntensity"].AsFloat = RenderSettings.ambientIntensity;

        JSONObject sun = new JSONObject();
        sun["intensity"].AsFloat = RenderSettings.sun.intensity;
        sun["color"] = WriteColor(RenderSettings.sun.color);
        sun["angle"] = WriteQuaternion(RenderSettings.sun.transform.rotation);

        lighting["sun"] = sun;

        return lighting;
    }

    private JSONObject WriteMap(VoxelArray voxelArray, List<string> materials, List<Substance> substances)
    {
        JSONObject map = new JSONObject();
        JSONArray voxels = new JSONArray();
        foreach (Voxel voxel in voxelArray.IterateVoxels())
        {
            if (voxel.IsEmpty())
            {
                Debug.Log("Empty voxel found!");
                voxelArray.VoxelModified(voxel);
                continue;
            }
            voxels[-1] = WriteVoxel(voxel, materials, substances);
        }
        map["voxels"] = voxels;
        return map;
    }

    private JSONObject WriteVoxel(Voxel voxel, List<string> materials, List<Substance> substances)
    {
        JSONObject voxelObject = new JSONObject();
        voxelObject["at"] = WriteIntVector3(voxel.transform.position);
        JSONArray faces = new JSONArray();
        for (int faceI = 0; faceI < voxel.faces.Length; faceI++)
        {
            VoxelFace face = voxel.faces[faceI];
            if (face.IsEmpty())
                continue;
            faces[-1] = WriteFace(face, faceI, materials);
        }
        voxelObject["f"] = faces;

        if (voxel.substance != null)
            voxelObject["s"].AsInt = substances.IndexOf(voxel.substance);

        return voxelObject;
    }

    private JSONObject WriteFace(VoxelFace face, int faceI, List<string> materials)
    {
        JSONObject faceObject = new JSONObject();
        faceObject["i"].AsInt = faceI;
        if (face.material != null)
        {
            faceObject["mat"].AsInt = materials.IndexOf(face.material.name);
        }
        if (face.overlay != null)
        {
            faceObject["over"].AsInt = materials.IndexOf(face.overlay.name);
        }
        if (face.orientation != 0)
        {
            faceObject["orient"].AsInt = face.orientation;
        }
        return faceObject;
    }

    private JSONArray WriteVector3(Vector3 v)
    {
        JSONArray a = new JSONArray();
        a[0] = v.x;
        a[1] = v.y;
        a[2] = v.z;
        return a;
    }

    private JSONArray WriteQuaternion(Quaternion v)
    {
        return WriteVector3(v.eulerAngles);
    }

    private JSONArray WriteIntVector3(Vector3 v)
    {
        JSONArray a = new JSONArray();
        a[0] = Mathf.RoundToInt(v.x);
        a[1] = Mathf.RoundToInt(v.y);
        a[2] = Mathf.RoundToInt(v.z);
        return a;
    }

    private JSONArray WriteColor(Color c)
    {
        JSONArray a = new JSONArray();
        a[0] = c.r;
        a[1] = c.g;
        a[2] = c.b;
        if (c.a != 1)
            a[3] = c.a;
        return a;
    }
}
